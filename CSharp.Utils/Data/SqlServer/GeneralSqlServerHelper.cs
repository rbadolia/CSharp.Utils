using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using CSharp.Utils.Data.Sync;
using CSharp.Utils.Properties;

namespace CSharp.Utils.Data.SqlServer
{
    public static class GeneralSqlServerHelper
    {
        #region Public Methods and Operators

        public static DateTime SqlServerAllowedMaxDateTime
        {
            get
            {
                return (DateTime)SqlDateTime.MaxValue;
            }
        }

        public static DateTime SqlServerAllowedMinDateTime
        {
            get
            {
                return (DateTime)SqlDateTime.MinValue;
            }
        }

        public static DateTime ToSqlServerAllowedDateTime(DateTime dateTime)
        {
            if (dateTime < SqlServerAllowedMinDateTime)
            {
                return SqlServerAllowedMinDateTime;
            }

            if (dateTime > SqlServerAllowedMaxDateTime)
            {
                return SqlServerAllowedMaxDateTime;
            }

            return dateTime;
        }

        public static List<string> GetTableDependencyOrder(SqlConnection connection, bool isOrdinaryDesc)
        {
            string s = " ORDER BY Ordinal" + (isOrdinaryDesc ? " DESC" : null) + ", TableName";
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = Resources.SQLSERVER_TABLE_DEPENDENCY_ORDER_COMMAND + s;
                using (IDataReader reader = command.ExecuteReader())
                {
                    var list = new List<string>();
                    while (reader.Read())
                    {
                        list.Add((string)reader[0]);
                    }

                    return list;
                }
            }
        }

        public static List<string> GetTableDependencyOrder(string connectionString, bool isOrdinaryDesc)
        {
            var connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                return GetTableDependencyOrder(connection, isOrdinaryDesc);
            }
        }

        public static void ImportData(string sourceConnectionString, string targetConnectionString, bool truncateTargetTable)
        {
            List<string> tableNames = GetTableDependencyOrder(sourceConnectionString, true);
            importData(sourceConnectionString, targetConnectionString, true, tableNames, false);
        }

        public static void ImportData(string sourceConnectionString, string targetConnectionString, bool truncateTargetTable, IList<string> tableNames)
        {
            importData(sourceConnectionString, targetConnectionString, true, tableNames, true);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void importData(string sourceConnectionString, string targetConnectionString, bool truncateTargetTable, IList<string> tableNames, bool reorder)
        {
            IList<string> tables = tableNames;
            if (reorder)
            {
                tables = GetTableDependencyOrder(sourceConnectionString, true);
                for (int i = 0; i < tables.Count; i++)
                {
                    if (!tableNames.Contains(tables[i]))
                    {
                        tables.RemoveAt(i);
                        i--;
                    }
                }
            }

            DataImporter.ImportData(typeof(SqlConnection), sourceConnectionString, typeof(SqlConnection), targetConnectionString, BulkCopier.Instance, truncateTargetTable, tables);
        }

        #endregion Methods
    }
}
