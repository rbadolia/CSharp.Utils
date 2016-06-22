using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CSharp.Utils.Data.Dynamic;

namespace CSharp.Utils.Data.Sync
{
    public static class SyncCommandHelper
    {
        #region Public Methods and Operators

        public static string GenerateInsertIntoTargetWithJoinScriptForDto(Type dtoType, string sourceTableName, string targetTableName)
        {
            IDbTableMetadata tableMeta = MetadataManager.GetMetaData(dtoType);

            List<KeyValuePair<string, string>> keyColumns;
            List<KeyValuePair<string, string>> nameColumns;
            List<KeyValuePair<string, string>> normalColumns;
            populateColumnInformation(tableMeta, out keyColumns, out nameColumns, out normalColumns);

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            for (int i = 0; i < normalColumns.Count; i++)
            {
                sb1.AppendLine("\t" + normalColumns[i].Value + ",");
                sb2.AppendLine("\t" + normalColumns[i].Key + ",");
            }

            for (int i = 0; i < nameColumns.Count; i++)
            {
                sb1.AppendLine("\t" + nameColumns[i].Value + ",");
                sb2.AppendLine("\t" + nameColumns[i].Key + ",");
            }

            string s1 = sb1.ToString().TrimEnd().TrimEnd(',');
            string s2 = sb2.ToString().TrimEnd().TrimEnd(',');
            var sb = new StringBuilder(string.Format("INSERT INTO {0} \r\n(\r\n", targetTableName));
            sb.Append(s1);
            sb.AppendLine(")");
            sb.AppendLine("SELECT");
            sb.Append(s2);
            sb.AppendLine();
            sb.AppendLine("FROM " + sourceTableName + " S");
            if (keyColumns.Count > 0)
            {
                sb.Append(" WHERE ");
                for (int i = 0; i < keyColumns.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(" AND ");
                    }

                    sb1.Append(string.Format("S.{0} IS NULL", keyColumns[i].Key));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GenerateUpdateKeysInSourceWithJoinScriptForDto(Type dtoType, string sourceTableName, string targetTableName)
        {
            IDbTableMetadata tableMeta = MetadataManager.GetMetaData(dtoType);

            List<KeyValuePair<string, string>> keyColumns;
            List<KeyValuePair<string, string>> nameColumns;
            List<KeyValuePair<string, string>> normalColumns;
            populateColumnInformation(tableMeta, out keyColumns, out nameColumns, out normalColumns);

            var sb = new StringBuilder("UPDATE S SET \r\n");
            for (int i = 0; i < keyColumns.Count; i++)
            {
                sb.AppendLine(string.Format("\tS.{0}=T.{1}", keyColumns[i].Value, keyColumns[i].Key));
                if (i < keyColumns.Count - 1)
                {
                    sb.Append(',');
                }
            }

            sb.AppendLine();

            sb.Append(string.Format("FROM {0} T INNER JOIN {1} S", targetTableName, sourceTableName));
            if (nameColumns.Count > 0)
            {
                sb.Append(" ON ");
                for (int i = 0; i < nameColumns.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(" AND ");
                    }

                    sb.Append(string.Format("T.{0}=S.{1}", nameColumns[i].Value, nameColumns[i].Key));
                }
            }

            return sb.ToString();
        }

        public static string GenerateUpdateTargetWithJoinScriptForDto(Type dtoType, string sourceTableName, string targetTableName)
        {
            IDbTableMetadata tableMeta = MetadataManager.GetMetaData(dtoType);

            List<KeyValuePair<string, string>> keyColumns;
            List<KeyValuePair<string, string>> nameColumns;
            List<KeyValuePair<string, string>> normalColumns;
            populateColumnInformation(tableMeta, out keyColumns, out nameColumns, out normalColumns);

            var sb = new StringBuilder("UPDATE T SET \r\n");
            for (int i = 0; i < normalColumns.Count; i++)
            {
                sb.AppendLine(string.Format("\tT.{0}=S.{1}", normalColumns[i].Value, normalColumns[i].Key));
                if (i < normalColumns.Count - 1)
                {
                    sb.Append(',');
                }
            }

            sb.AppendLine();

            sb.Append(string.Format("FROM {0} T INNER JOIN {1} S", targetTableName, sourceTableName));
            if (keyColumns.Count > 0)
            {
                var sb1 = new StringBuilder("WHERE ");
                for (int i = 0; i < keyColumns.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(" AND ");
                        sb1.Append(" AND ");
                    }

                    sb.Append(string.Format("T.{0}=S.{1}", keyColumns[i].Value, keyColumns[i].Key));
                    sb1.Append(string.Format("S.{0} IS NOT NULL", keyColumns[i].Key));
                }

                sb.AppendLine();
                sb.Append(sb1);
            }

            return sb.ToString();
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void populateColumnInformation(IDbTableMetadata tableMeta, out List<KeyValuePair<string, string>> keyColumns, out List<KeyValuePair<string, string>> nameColumns, out List<KeyValuePair<string, string>> normalColumns)
        {
            keyColumns = new List<KeyValuePair<string, string>>();
            nameColumns = new List<KeyValuePair<string, string>>();
            normalColumns = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < tableMeta.ColumnsMetaData.Count; i++)
            {
                IDbColumnMetadata meta = tableMeta.ColumnsMetaData[i];
                if (meta.Property.IsDefined(typeof(NotSyncColumnAttribute)))
                {
                    continue;
                }

                var keyAttrib = meta.Property.GetCustomAttribute<KeyColumnAttribute>(true);
                if (keyAttrib != null)
                {
                    keyColumns.Add(new KeyValuePair<string, string>("[" + meta.ColumnName + "]", "[" + (keyAttrib.TargetColumnName ?? meta.ColumnName) + "]"));
                    continue;
                }

                var nameAttrib = meta.Property.GetCustomAttribute<NameColumnAttribute>(true);
                if (nameAttrib != null)
                {
                    nameColumns.Add(new KeyValuePair<string, string>("[" + meta.ColumnName + "]", "[" + (nameAttrib.TargetColumnName ?? meta.ColumnName) + "]"));
                    continue;
                }

                string targetColumnName = meta.ColumnName;

                var mappingAttrib = meta.Property.GetCustomAttribute<ColumnMappingAttribute>(true);
                if (mappingAttrib != null)
                {
                    targetColumnName = mappingAttrib.TargetColumnName ?? meta.ColumnName;
                }

                normalColumns.Add(new KeyValuePair<string, string>("[" + meta.ColumnName + "]", "[" + targetColumnName + "]"));
            }
        }

        #endregion Methods
    }
}
