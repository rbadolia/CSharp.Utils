using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Data.Helpers.Internal;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Data.Helpers
{
    public static class DynamicToDataReaderDataTableHelper<T>
        where T : class
    {
        #region Static Fields

        private static readonly List<KeyValuePair<string, Type>> _columns = new List<KeyValuePair<string, Type>>();

        private static readonly PopulateItemArrayDelegate<T> _populateItemArrayDelegate;

        private static DataTable _schema;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicToDataReaderDataTableHelper()
        {
            Type objectType = typeof(T);
            var dynamicMethod = new DynamicMethod("populateItemArray", typeof(void), new[] { objectType, typeof(object[]) }, typeof(DynamicToDataReaderDataTableHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties<T>(true);
            int propertyIndex = 0;
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && (!objectType.IsInterface || !doesColumnExist(property.Name)))
                {
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldc_I4_S, propertyIndex);
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                    if (property.PropertyType.IsEnum)
                    {
                        ilGen.Emit(OpCodes.Box, property.PropertyType);
                        ilGen.Emit(OpCodes.Call, SharedReflectionInfo.ObjectToStringMethod);
                    }
                    else
                    {
                        if (property.PropertyType.IsValueType)
                        {
                            ilGen.Emit(OpCodes.Box, property.PropertyType);
                        }
                    }

                    ilGen.Emit(OpCodes.Callvirt, SharedReflectionInfo.ArrayIndexerSetMethod);
                    propertyIndex++;
                    _columns.Add(new KeyValuePair<string, Type>(property.Name, ToDataReaderDataTableHelper.DataTableSupportedDataTypes.Contains(property.PropertyType) ? property.PropertyType : typeof(object)));
                }
            }

            ilGen.Emit(OpCodes.Ret);
            _populateItemArrayDelegate = (PopulateItemArrayDelegate<T>)dynamicMethod.CreateDelegate(typeof(PopulateItemArrayDelegate<T>));
            _schema = ToDataTableSchema();
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public static IDataReader ExportAsDataReader(IEnumerable<T> enumerable)
        {
            var strategy = new EnumeratorBasedDataReaderStrategy<T>(enumerable.GetEnumerator(), _columns, _populateItemArrayDelegate);
            strategy.Initialize();
            var reader = new GenericDataReader(strategy);
            return reader;
        }

        public static IEnumerable<DataRow> ExportAsDataRows(IEnumerable<T> enumerable)
        {
            if (enumerable != null)
            {
                var itemArray = new object[_columns.Count];
                foreach (T item in enumerable)
                {
                    _populateItemArrayDelegate(item, itemArray);
                    DataRow row = _schema.NewRow();
                    row.ItemArray = itemArray;
                    yield return row;
                }
            }
        }

        [CautionUsedByReflection]
        public static DataTable ExportAsDataTable(IEnumerable<T> enumerable)
        {
            DataTable table = ToDataTableSchema();
            var itemArray = new object[table.Columns.Count];
            if (enumerable != null)
            {
                foreach (T item in enumerable)
                {
                    _populateItemArrayDelegate(item, itemArray);
                    table.Rows.Add(itemArray);
                }
            }

            return table;
        }

        public static List<KeyValuePair<string, Type>> GetColumnTypes()
        {
            var list = new List<KeyValuePair<string, Type>>();
            foreach (var kvp in _columns)
            {
                list.Add(new KeyValuePair<string, Type>(kvp.Key, kvp.Value));
            }

            return list;
        }

        [CautionUsedByReflection]
        public static DataTable ToDataTableSchema()
        {
            var table = new DataTable();
            foreach (var kvp in _columns)
            {
                table.Columns.Add(kvp.Key, kvp.Value);
            }

            table.RemotingFormat = SerializationFormat.Binary;
            return table;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static bool doesColumnExist(string columnName)
        {
            foreach (var kvp in _columns)
            {
                if (kvp.Key == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Methods
    }
}
