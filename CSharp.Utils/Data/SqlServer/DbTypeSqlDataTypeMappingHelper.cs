using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.SqlServer
{
    public static class DbTypeSqlDataTypeMappingHelper
    {
        #region Constants

        public const string SqlDataTypeForUnknownTypes = "sql_variant";

        #endregion Constants

        #region Static Fields

        private static readonly Dictionary<DbType, string> TypeMappings;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DbTypeSqlDataTypeMappingHelper()
        {
            TypeMappings = new Dictionary<DbType, string>();
            TypeMappings.Add(DbType.AnsiString, "varchar");
            TypeMappings.Add(DbType.AnsiStringFixedLength, "char");
            TypeMappings.Add(DbType.Binary, "varbinary");
            TypeMappings.Add(DbType.Boolean, "bit");
            TypeMappings.Add(DbType.Byte, "tinyint");
            TypeMappings.Add(DbType.Date, "date");
            TypeMappings.Add(DbType.DateTime, "datetime");
            TypeMappings.Add(DbType.DateTime2, "datetime2");
            TypeMappings.Add(DbType.DateTimeOffset, "datetimeoffset");
            TypeMappings.Add(DbType.Decimal, "decimal");
            TypeMappings.Add(DbType.Double, "float");
            TypeMappings.Add(DbType.Guid, "uniqueidentifier");
            TypeMappings.Add(DbType.Int16, "smallint");
            TypeMappings.Add(DbType.Int32, "int");
            TypeMappings.Add(DbType.Int64, "bigint");
            TypeMappings.Add(DbType.Object, "sql_variant");
            TypeMappings.Add(DbType.Single, "real");
            TypeMappings.Add(DbType.String, "nvarchar");
            TypeMappings.Add(DbType.StringFixedLength, "nchar");
            TypeMappings.Add(DbType.Time, "time");
            TypeMappings.Add(DbType.Xml, "xml");
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public static string GetSqlDataType(DbType dbType)
        {
            string result;
            if (!TypeMappings.TryGetValue(dbType, out result))
            {
                result = SqlDataTypeForUnknownTypes;
            }

            return result;
        }

        #endregion Public Methods and Operators
    }
}
