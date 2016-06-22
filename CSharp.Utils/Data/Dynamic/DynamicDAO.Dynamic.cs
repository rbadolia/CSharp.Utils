using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CSharp.Utils.Data.Dynamic.Internal;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        #region Static Fields

        private static populateEntityFromRecordDelegate _populateEntityFromRecord;

        private static populateValuesDelegate _populateValuesFromEntityForInsert;

        private static populateValuesDelegate _populateValuesFromEntityForUpdate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicDAO()
        {
            Type entityType = typeof(E);
            IDbTableMetadata data = MetadataManager.GetMetaData(entityType);
            if (data == null)
            {
                throw new Exception("Cannot build DynamicDAO for this entity. No Meta-data is available. The entity type is: " + entityType);
            }

            TableName = data.TableName;
            _metaData = new MetaData();
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (IDbColumnMetadata cm in data.ColumnsMetaData)
            {
                foreach (PropertyInfo property in properties)
                {
                    if (cm.Property == property)
                    {
                        if (cm.IsKey)
                        {
                            _metaData.KeyProperties.Add(new KeyValuePair<PropertyInfo, IDbColumnMetadata>(property, cm));
                        }
                        else
                        {
                            _metaData.NonKeyProperties.Add(new KeyValuePair<PropertyInfo, IDbColumnMetadata>(property, cm));
                        }
                    }
                }
            }

            _metaData.NonKeyProperties.Sort(DbColumnComparer.Instance);
            _metaData.KeyProperties.Sort(DbColumnComparer.Instance);
            foreach (var keyPropertyPair in _metaData.KeyProperties)
            {
                _allColumnNames.Add(keyPropertyPair.Value.ColumnName);
                _keyColumnNames.Add(keyPropertyPair.Value.ColumnName);
            }

            foreach (var nonKeyPropertyPair in _metaData.NonKeyProperties)
            {
                _allColumnNames.Add(nonKeyPropertyPair.Value.ColumnName);
            }

            buildInsert();
            buildUpdate();
            buildSelect();
            _selectOneCommandFormat = _selectCommandFormat;
            _deleteCommandFormat = "DELETE FROM {0}";
            _deleteCommandIfNoKeyFormat = _deleteCommandFormat;
            if (_metaData.KeyProperties.Count > 0)
            {
                _selectOneCommandFormat += " WHERE ";
                string s = null;
                for (int i = 0; i < _metaData.KeyProperties.Count; i++)
                {
                    s += "[" + _metaData.KeyProperties[i].Value.ColumnName + "]" + "=@" + _metaData.KeyProperties[i].Value.ColumnName;
                    if (i < _metaData.KeyProperties.Count - 1)
                    {
                        s += " AND ";
                    }
                }

                _selectOneCommandFormat += s;
                _deleteCommandFormat += " WHERE " + s;
            }
            else
            {
                _deleteCommandIfNoKeyFormat = _deleteCommandFormat + " WHERE " + _updateCommandFormat.Substring(14).Replace(",", " AND ");
            }

            string tn = "[" + TableName + "]";
            _selectCommand = string.Format(_selectCommandFormat, tn);
            _selectOneCommand = string.Format(_selectOneCommandFormat, tn);
            _insertCommand = string.Format(_insertCommandFormat, tn);
            _updateCommand = string.Format(_updateCommandFormat, tn);
            _deleteCommand = string.Format(_deleteCommandFormat, tn);
            _deleteCommandIfNoKey = string.Format(_deleteCommandIfNoKeyFormat, tn);

            SelectStatement = _selectCommand;
            InsertStatement = _insertCommand;
            UpdateStatement = _updateCommand;
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void populateEntityFromRecordDelegate(IDataRecord record, E entity);

        private delegate void populateValuesDelegate(object[] array, E entity);

        #endregion Delegates

        #region Methods

        private static void buildInsert()
        {
            var populateValuesFromEntityForInsertDynamicMethod = new DynamicMethod("populateValuesFromEntityForInsertDynamicMethod", typeof(void), new[] { typeof(object[]), typeof(E) }, typeof(DynamicDAO<E>));
            ILGenerator populateValuesFromEntityForInsertIlGen = populateValuesFromEntityForInsertDynamicMethod.GetILGenerator();

            var allProperties = new List<KeyValuePair<PropertyInfo, IDbColumnMetadata>>();
            allProperties.AddRange(_metaData.KeyProperties);
            allProperties.AddRange(_metaData.NonKeyProperties);

            var columnNames = new StringBuilder();
            var parameterNames = new StringBuilder();
            for (int i = 0; i < allProperties.Count; i++)
            {
                populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Ldarg_0);
                populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Ldc_I4, i);
                populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Ldarg_1);
                populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Callvirt, allProperties[i].Key.GetMethod);
                if (allProperties[i].Key.PropertyType.IsValueType)
                {
                    populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Box, allProperties[i].Key.PropertyType);
                }

                populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Callvirt, SharedReflectionInfo.ArrayIndexerSetMethod);
                columnNames.Append("[" + allProperties[i].Value.ColumnName + "]");
                parameterNames.Append("@" + allProperties[i].Value.ColumnName);
                columnNames.Append(", ");
                parameterNames.Append(", ");
            }

            columnNames.Length -= 2;
            parameterNames.Length -= 2;
            populateValuesFromEntityForInsertIlGen.Emit(OpCodes.Ret);
            _insertCommandFormat = "INSERT INTO {0} " + string.Format("({0}) VALUES ({1})", columnNames, parameterNames);

            _populateValuesFromEntityForInsert = (populateValuesDelegate)populateValuesFromEntityForInsertDynamicMethod.CreateDelegate(typeof(populateValuesDelegate));
        }

        private static void buildSelect()
        {
            var populateEntityFromRecordDynamicMethod = new DynamicMethod("populateEntityFromRecordDynamicMethod", typeof(void), new[] { typeof(IDataRecord), typeof(E) }, typeof(DynamicDAO<E>));
            ILGenerator populateEntityFromRecordIlGen = populateEntityFromRecordDynamicMethod.GetILGenerator();
            var allProperties = new List<KeyValuePair<PropertyInfo, IDbColumnMetadata>>();
            allProperties.AddRange(_metaData.KeyProperties);
            allProperties.AddRange(_metaData.NonKeyProperties);

            var columnNames = new StringBuilder();
            for (int i = 0; i < allProperties.Count; i++)
            {
                populateEntityFromRecordIlGen.Emit(OpCodes.Ldarg_1);
                populateEntityFromRecordIlGen.Emit(OpCodes.Ldarg_0);
                populateEntityFromRecordIlGen.Emit(OpCodes.Ldc_I4, i);
                MethodInfo getValueConcreteMethod = SharedReflectionInfo.GetValueFromDataRecordMethod.MakeGenericMethod(allProperties[i].Key.PropertyType);
                populateEntityFromRecordIlGen.Emit(OpCodes.Call, getValueConcreteMethod);
                populateEntityFromRecordIlGen.Emit(OpCodes.Callvirt, allProperties[i].Key.SetMethod);
                columnNames.Append("[" + allProperties[i].Value.ColumnName + "]");
                columnNames.Append(", ");
            }

            columnNames.Length -= 2;
            populateEntityFromRecordIlGen.Emit(OpCodes.Ret);
            _selectCommandFormat = string.Format("SELECT {0} FROM {{0}}", columnNames);
            _populateEntityFromRecord = (populateEntityFromRecordDelegate)populateEntityFromRecordDynamicMethod.CreateDelegate(typeof(populateEntityFromRecordDelegate));
        }

        private static void buildUpdate()
        {
            var populateValuesFromEntityForUpdateDynamicMethod = new DynamicMethod("populateValuesFromEntityForUpdateDynamicMethod", typeof(void), new[] { typeof(object[]), typeof(E) }, typeof(DynamicDAO<E>));
            ILGenerator populateValuesFromEntityForUpdateIlGen = populateValuesFromEntityForUpdateDynamicMethod.GetILGenerator();
            var allProperties = new List<KeyValuePair<PropertyInfo, IDbColumnMetadata>>();
            allProperties.AddRange(_metaData.NonKeyProperties);
            allProperties.AddRange(_metaData.KeyProperties);
            var columnNames = new StringBuilder("UPDATE {0} SET ");
            for (int i = 0; i < allProperties.Count; i++)
            {
                populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Ldarg_0);
                populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Ldc_I4, i);
                populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Ldarg_1);
                populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Callvirt, allProperties[i].Key.GetMethod);
                if (allProperties[i].Key.PropertyType.IsValueType)
                {
                    populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Box, allProperties[i].Key.PropertyType);
                }

                populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Callvirt, SharedReflectionInfo.ArrayIndexerSetMethod);
                if (_metaData.KeyProperties.Count > 0 && i == _metaData.NonKeyProperties.Count)
                {
                    columnNames.Length -= 2;
                    columnNames.Append(" WHERE ");
                }

                columnNames.Append(allProperties[i].Value.ColumnName + "=@" + allProperties[i].Value.ColumnName);
                columnNames.Append(", ");
            }

            columnNames.Length -= 2;
            _updateCommandFormat = columnNames.ToString();
            populateValuesFromEntityForUpdateIlGen.Emit(OpCodes.Ret);

            _populateValuesFromEntityForUpdate = (populateValuesDelegate)populateValuesFromEntityForUpdateDynamicMethod.CreateDelegate(typeof(populateValuesDelegate));
        }

        #endregion Methods
    }
}
