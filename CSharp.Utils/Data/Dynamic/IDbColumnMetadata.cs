using System.Data;
using System.Reflection;

namespace CSharp.Utils.Data.Dynamic
{
    public interface IDbColumnMetadata
    {
        #region Public Properties

        DbColumnAccessTypes AccessType { get; }

        DbType ColumnDbType { get; }

        string ColumnName { get; }

        string FalseValueText { get; }

        bool IsKey { get; }

        int? MaxLength { get; }

        bool IsNullable { get; }

        PropertyInfo Property { get; }

        string TrueValueText { get; }

        #endregion Public Properties
    }
}
