using System;
using System.Data;
using System.Reflection;

namespace CSharp.Utils.Data.Dynamic
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DbColumnAttribute : Attribute, IDbColumnMetadata
    {
        #region Constructors and Finalizers

        public DbColumnAttribute(string columnName, bool isKey)
        {
            this.ColumnName = columnName;
            this.IsKey = isKey;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public DbColumnAccessTypes AccessType { get; set; }

        public DbType ColumnDbType { get; set; }

        public string ColumnName { get; private set; }

        public string FalseValueText { get; set; }

        public bool IsKey { get; private set; }

        public int? MaxLength { get; set; }

        public bool IsNullable { get; set; }

        public PropertyInfo Property { get; set; }

        public string TrueValueText { get; set; }

        #endregion Public Properties
    }
}
