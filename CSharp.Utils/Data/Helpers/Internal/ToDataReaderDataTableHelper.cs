using System;
using System.Collections.Generic;

namespace CSharp.Utils.Data.Helpers.Internal
{
    internal static class ToDataReaderDataTableHelper
    {
        #region Static Fields

        internal static readonly HashSet<Type> DataTableSupportedDataTypes;

        #endregion Static Fields

        #region Constructors and Finalizers

        static ToDataReaderDataTableHelper()
        {
            DataTableSupportedDataTypes = new HashSet<Type>();
            DataTableSupportedDataTypes.Add(typeof(bool));
            DataTableSupportedDataTypes.Add(typeof(byte));
            DataTableSupportedDataTypes.Add(typeof(char));
            DataTableSupportedDataTypes.Add(typeof(DateTime));
            DataTableSupportedDataTypes.Add(typeof(decimal));
            DataTableSupportedDataTypes.Add(typeof(double));
            DataTableSupportedDataTypes.Add(typeof(Guid));
            DataTableSupportedDataTypes.Add(typeof(short));
            DataTableSupportedDataTypes.Add(typeof(int));
            DataTableSupportedDataTypes.Add(typeof(long));
            DataTableSupportedDataTypes.Add(typeof(sbyte));
            DataTableSupportedDataTypes.Add(typeof(float));
            DataTableSupportedDataTypes.Add(typeof(string));
            DataTableSupportedDataTypes.Add(typeof(TimeSpan));
            DataTableSupportedDataTypes.Add(typeof(ushort));
            DataTableSupportedDataTypes.Add(typeof(uint));
            DataTableSupportedDataTypes.Add(typeof(ulong));
            DataTableSupportedDataTypes.Add(typeof(byte[]));
        }

        #endregion Constructors and Finalizers
    }
}
