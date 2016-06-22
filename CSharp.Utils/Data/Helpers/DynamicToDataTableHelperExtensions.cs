using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CSharp.Utils.Data.Helpers
{
    public static class DynamicToDataTableHelperExtensions
    {
        #region Public Methods and Operators

        public static DataTable ExportAsDataTable(this IQueryable queryable)
        {
            return ExportAsDataTable(queryable, queryable.ElementType);
        }

        public static DataTable ExportAsDataTable(this IEnumerable enumerable, Type elementType)
        {
            Type toDataTableType = typeof(DynamicToDataReaderDataTableHelper<>).MakeGenericType(elementType);
            MethodInfo mi = toDataTableType.GetMethod("ExportAsDataTable");
            var table = mi.Invoke(null, new object[] { enumerable }) as DataTable;
            return table;
        }

        public static DataTable ExportAsDataTable<T>(this IEnumerable<T> enumerable) where T : class
        {
            return DynamicToDataReaderDataTableHelper<T>.ExportAsDataTable(enumerable);
        }

        #endregion Public Methods and Operators
    }
}
