using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace CSharp.Utils.Data.Helpers
{
    public static class DynamicToDataSetHelperExtensions
    {
        #region Public Methods and Operators

        public static void AddToDataSet<T>(IEnumerable<T> enumerable, DataSet dataSet, string dataTableName) where T : class
        {
            DataTable table = DynamicToDataReaderDataTableHelper<T>.ExportAsDataTable(enumerable);
            table.TableName = dataTableName;
            dataSet.Tables.Add(table);
        }

        public static DataSet ExportAsDataSet<T>(T obj)
        {
            return DynamicToDataSetHelper<T>.ExportAsDataSet(obj);
        }

        public static DataSet ExportAsDataSet(object obj)
        {
            Type toDataSetType = typeof(DynamicToDataSetHelper<>).MakeGenericType(obj.GetType());
            MethodInfo mi = toDataSetType.GetMethod("ExportAsDataSet");
            var dataSet = mi.Invoke(null, new[] { obj }) as DataSet;
            return dataSet;
        }

        #endregion Public Methods and Operators
    }
}
