using System;
using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.Sync
{
    public static class DataImporter
    {
        #region Public Methods and Operators

        public static void ImportData(IDbConnection sourceConnection, IDbConnection targetConnection, IBulkCopier bulkCopier, bool truncateTargetTable, IList<string> tableNames)
        {
            if (truncateTargetTable)
            {
                IDbCommand targetCommand = targetConnection.CreateCommand();
                foreach (string tableName in tableNames)
                {
                    targetCommand.CommandText = "DELETE [" + tableName + "]";
                    targetCommand.ExecuteNonQuery();
                }
            }

            for (int i = tableNames.Count - 1; i >= 0; i--)
            {
                IDbCommand command = sourceConnection.CreateCommand();
                command.CommandText = "SELECT * FROM [" + tableNames[i] + "]";
                IDataReader reader = command.ExecuteReader();

                bulkCopier.DoBulkCopy(reader, targetConnection, tableNames[i]);
                reader.Dispose();
            }
        }

        public static void ImportData(Type sourceDbConnectionType, string sourceConnectionString, Type targetDbConnectionType, string targetConnectionString, IBulkCopier bulkCopier, bool truncateTargetTable, IList<string> tableNames)
        {
            var sourceConnection = (IDbConnection)Activator.CreateInstance(sourceDbConnectionType);
            sourceConnection.ConnectionString = sourceConnectionString;

            var targetConnection = (IDbConnection)Activator.CreateInstance(targetDbConnectionType);
            targetConnection.ConnectionString = targetConnectionString;
            using (sourceConnection)
            {
                sourceConnection.Open();
                using (targetConnection)
                {
                    targetConnection.Open();
                    ImportData(sourceConnection, targetConnection, bulkCopier, truncateTargetTable, tableNames);
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
