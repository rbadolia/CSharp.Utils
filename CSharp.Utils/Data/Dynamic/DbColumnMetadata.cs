using System.Data;
using System.Reflection;

namespace CSharp.Utils.Data.Dynamic
{
    public class DbColumnMetadata : IDbColumnMetadata
    {
        #region Constructors and Finalizers

        public DbColumnMetadata(string columnName, bool isKey)
        {
            this.ColumnName = columnName;
            this.IsKey = isKey;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public DbColumnAccessTypes AccessType { get; set; }

        public DbType ColumnDbType { get; set; }

        public string ColumnName { get; set; }

        public string FalseValueText { get; set; }

        public bool IsKey { get; set; }

        public int? MaxLength { get; set; }

        public bool IsNullable { get; set; }

        public PropertyInfo Property { get; set; }

        public string TrueValueText { get; set; }

        #endregion Public Properties
    }
}
