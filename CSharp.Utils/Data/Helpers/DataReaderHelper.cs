using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Data.Helpers
{
    public delegate void PopulateObjectFromDataRecordCallback<in T>(T obj, IDataRecord record);

    public static class DataReaderHelper
    {
        #region Public Methods and Operators

        public static PopulateObjectFromDataRecordCallback<T> BuildDynamicMethodForPopulatingObject<T>(IDataRecord record)
        {
            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < record.FieldCount; i++)
            {
                hashSet.Add(record.GetName(i));
            }

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var propList = new List<KeyValuePair<PropertyInfo, int>>();
            foreach (PropertyInfo property in properties)
            {
                var attrib = property.GetCustomAttribute<XmlElementAttribute>();
                string fieldName = (attrib == null || string.IsNullOrEmpty(attrib.ElementName)) ? property.Name : attrib.ElementName;
                if (fieldName != null && hashSet.Contains(fieldName))
                {
                    int ordinal = record.GetOrdinal(fieldName);
                    propList.Add(new KeyValuePair<PropertyInfo, int>(property, ordinal));
                }
            }

            var dynamicMethod = new DynamicMethod("populateObjectFromDataRecord" + GeneralHelper.Identity, typeof(void), new[] { typeof(T), typeof(IDataRecord) }, typeof(DataReaderHelper));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            foreach (var kvp in propList)
            {
                Type propertyType = kvp.Key.PropertyType;
                MethodInfo convertMethod = null;
                int convertMode = -1;
                Type nullableType = null;
                if (!propertyType.IsEnum)
                {
                    Type fieldType = record.GetFieldType(kvp.Value);
                    if (propertyType == fieldType || propertyType == typeof(object) || (fieldType.IsValueType && propertyType == typeof(Nullable<>).MakeGenericType(fieldType)))
                    {
                        convertMode = 0;
                    }

                    if (convertMode == -1 && propertyType.IsValueType)
                    {
                        Type t = propertyType;

                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            t = propertyType.GetGenericArguments()[0];
                            nullableType = t;
                        }

                        if (SharedReflectionInfo.ConvertToMethods.TryGetValue(t, out convertMethod))
                        {
                            convertMode = 1;
                        }
                    }

                    if (convertMode == -1)
                    {
                        MethodInfo mi = propertyType.GetMethod("Parse", new[] { typeof(string) });

                        if (mi != null && mi.IsStatic && mi.ReflectedType == propertyType)
                        {
                            ParameterInfo[] parameters = mi.GetParameters();
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                            {
                                convertMode = 2;
                                convertMethod = mi;
                            }
                        }
                    }
                }
                else
                {
                    convertMethod = typeof(ReflectionHelper).GetMethod("ParseEnum").MakeGenericMethod(propertyType);
                    convertMode = 3;
                }

                if (convertMode != -1)
                {
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldc_I4, kvp.Value);
                    MethodInfo getValueConcreteMethod = SharedReflectionInfo.GetValueFromDataRecordMethod.MakeGenericMethod(convertMode == 0 ? propertyType : typeof(object));
                    ilGen.Emit(OpCodes.Call, getValueConcreteMethod);
                    if (convertMode != 0)
                    {
                        ilGen.Emit(OpCodes.Call, SharedReflectionInfo.ObjectToStringMethod);
                        ilGen.Emit(OpCodes.Call, convertMethod);
                        if (convertMode == 3)
                        {
                            ilGen.Emit(OpCodes.Unbox_Any, propertyType);
                        }
                    }

                    if (nullableType != null)
                    {
                        ConstructorInfo c = typeof(Nullable<>).MakeGenericType(nullableType).GetConstructor(new[] { nullableType });
                        ilGen.Emit(OpCodes.Newobj, c);
                    }

                    ilGen.Emit(OpCodes.Callvirt, kvp.Key.SetMethod);
                }
            }

            ilGen.Emit(OpCodes.Ret);

            return (PopulateObjectFromDataRecordCallback<T>)dynamicMethod.CreateDelegate(typeof(PopulateObjectFromDataRecordCallback<T>));
        }

        public static IEnumerable<T> ConvertDataReaderToObjects<T>(this IDataReader reader) where T : new()
        {
            PopulateObjectFromDataRecordCallback<T> callback = BuildDynamicMethodForPopulatingObject<T>(reader);
            return ConvertDataReaderToObjects(reader, callback);
        }

        public static IEnumerable<T> ConvertDataReaderToObjects<T>(this IDataReader reader, PopulateObjectFromDataRecordCallback<T> callback) where T : new()
        {
            while (reader.Read())
            {
                var obj = new T();
                callback(obj, reader);
                yield return obj;
            }

            reader.Dispose();
        }

        public static IDataReader ExportAsDataReader<T>(this IEnumerable<T> enumerable) where T : class
        {
            return DynamicToDataReaderDataTableHelper<T>.ExportAsDataReader(enumerable);
        }

        public static DataTable ExportAsDataTable(this IDataReader reader, string tableName)
        {
            var table = new DataTable(tableName);
            table.Load(reader);
            return table;
        }

        public static IDataReader GetDataFromCharSeparatedFile(string fileName, char delimiter, IList<KeyValuePair<string, Type>> columns)
        {
            var reader = new StreamReader(fileName);
            return GetDataFromCharSeparatedTextReader(reader, delimiter, columns);
        }

        public static IDataReader GetDataFromCharSeparatedFile(string fileName, char delimiter, IList<string> columnNames)
        {
            var reader = new StreamReader(fileName);
            return GetDataFromCharSeparatedTextReader(reader, delimiter, columnNames);
        }

        public static IDataReader GetDataFromCharSeparatedFile(string fileName, char delimiter)
        {
            var reader = new StreamReader(fileName);
            return GetDataFromCharSeparatedTextReader(reader, delimiter);
        }

        public static IDataReader GetDataFromCharSeparatedTextReader(TextReader reader, char delimiter, IList<KeyValuePair<string, Type>> columns)
        {
            var enumerator = new TextReaderEnumerator(reader);
            var strategy = new CharSeparatedDataReaderStrategy(enumerator, delimiter, columns);
            strategy.Initialize();
            var dataReader = new GenericDataReader(strategy);
            return dataReader;
        }

        public static IDataReader GetDataFromCharSeparatedTextReader(TextReader reader, char delimiter, IList<string> columnNames)
        {
            var enumerator = new TextReaderEnumerator(reader);
            var strategy = new CharSeparatedDataReaderStrategy(enumerator, delimiter, columnNames);
            strategy.Initialize();
            var dataReader = new GenericDataReader(strategy);
            return dataReader;
        }

        public static IDataReader GetDataFromCharSeparatedTextReader(TextReader reader, char delimiter)
        {
            var enumerator = new TextReaderEnumerator(reader);
            var strategy = new CharSeparatedDataReaderStrategy(enumerator, delimiter);
            strategy.Initialize();
            var dataReader = new GenericDataReader(strategy);
            return dataReader;
        }

        #endregion Public Methods and Operators
    }
}
