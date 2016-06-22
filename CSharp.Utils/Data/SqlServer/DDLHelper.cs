using System;
using System.Text;
using CSharp.Utils.Data.Dynamic;

namespace CSharp.Utils.Data.SqlServer
{
    public static class DDLHelper
    {
        #region Public Methods and Operators

        public static string GenerateCreateTableScriptForDto(Type modelType, string tableName, bool addConstraints)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = modelType.Name;
            }

            IDbTableMetadata meta = MetadataManager.GetMetaData(modelType);
            var sb = new StringBuilder("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append("\r\n(\r\n");
            for (int i = 0; i < meta.ColumnsMetaData.Count; i++)
            {
                IDbColumnMetadata columnMeta = meta.ColumnsMetaData[i];
                string mappedType = DbTypeSqlDataTypeMappingHelper.GetSqlDataType(columnMeta.ColumnDbType);
                if (mappedType == DbTypeSqlDataTypeMappingHelper.SqlDataTypeForUnknownTypes)
                {
                    continue;
                }

                sb.Append(string.Format("\t[{0}] {1}", columnMeta.ColumnName, mappedType));
                if (columnMeta.Property.PropertyType == typeof(byte[]) || columnMeta.Property.PropertyType == typeof(char[]) || columnMeta.Property.PropertyType == typeof(string))
                {
                    int maxLength = columnMeta.MaxLength == null ? 100 : columnMeta.MaxLength.Value;
                    sb.Append("(" + maxLength + ")");
                }

                if (addConstraints)
                {
                    if (!columnMeta.IsNullable)
                    {
                        sb.Append(" NOT NULL");
                    }
                }

                sb.Append(",\r\n");
            }

            return sb.ToString().TrimEnd().TrimEnd(',') + "\r\n)";
        }

        public static string GenerateCreateTableScriptForDto<T>(string tableName, bool addConstraints)
        {
            return GenerateCreateTableScriptForDto(typeof(T), tableName, addConstraints);
        }

        #endregion Public Methods and Operators
    }
}
