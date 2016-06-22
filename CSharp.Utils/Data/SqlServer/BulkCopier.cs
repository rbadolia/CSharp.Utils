using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CSharp.Utils.Data.Dynamic;
using CSharp.Utils.Data.Helpers;

namespace CSharp.Utils.Data.SqlServer
{
    public sealed class BulkCopier : IBulkCopier
    {
        #region Static Fields

        private static readonly BulkCopier InstanceObject = new BulkCopier();

        #endregion Static Fields

        #region Constructors and Finalizers

        private BulkCopier()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static BulkCopier Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void DoBulkCopy(IDataReader reader, IDbConnection connection, string tableName)
        {
            doTypeChecks(connection, null);
            this.DoBulkCopy(reader, connection, null, tableName);
        }

        public void DoBulkCopy(IDataReader reader, IDbTransaction transaction, string tableName)
        {
            doTypeChecks(null, transaction);
            this.DoBulkCopy(reader, null, transaction, tableName);
        }

        public void DoBulkCopy<T>(IDbConnection connection, IEnumerable<T> enumerable, string tableName = null) where T : class
        {
            this.DoBulkCopy(enumerable, connection, null, tableName);
        }

        public void DoBulkCopy<T>(IEnumerable<T> enumerable, IDbTransaction transaction, string tableName = null) where T : class
        {
            this.DoBulkCopy(enumerable, null, transaction, tableName);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void CheckConnectionType(IDbConnection connection)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection == null)
            {
                throw new ArgumentException(@"The given connection is not SqlConnection type", "connection");
            }
        }

        private static void CheckTransactionType(IDbTransaction transaction)
        {
            if (transaction != null)
            {
                var sqlTransaction = transaction as SqlTransaction;
                if (sqlTransaction == null)
                {
                    throw new ArgumentException(@"The given transaction is not SqlTransaction type", "transaction");
                }
            }
        }

        private static void doTypeChecks(IDbConnection connection, IDbTransaction transaction)
        {
            if (connection != null)
            {
                CheckConnectionType(connection);
            }
            else
            {
                CheckConnectionType(transaction.Connection);
                CheckTransactionType(transaction);
            }
        }

        private static string getDefaultTableName<T>()
        {
            IDbTableMetadata meta = MetadataManager.GetMetaData(typeof(T));
            return meta.TableName;
        }

        private void DoBulkCopy(IDataReader reader, IDbConnection connection, IDbTransaction transaction, string tableName)
        {
            var sqlConnection = connection as SqlConnection;
            var sqlTransaction = transaction as SqlTransaction;

            SqlBulkCopy sqlBulkCopy = sqlConnection != null ? new SqlBulkCopy(sqlConnection) : new SqlBulkCopy(sqlTransaction.Connection, SqlBulkCopyOptions.Default, sqlTransaction);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                sqlBulkCopy.ColumnMappings.Add(columnName, columnName);
            }

            sqlBulkCopy.DestinationTableName = tableName;

            sqlBulkCopy.WriteToServer(reader);
        }

        private void DoBulkCopy<T>(IEnumerable<T> enumerable, IDbConnection connection, IDbTransaction transaction, string tableName) where T : class
        {
            doTypeChecks(connection, transaction);
            if (tableName == null)
            {
                tableName = getDefaultTableName<T>();
            }

            IDataReader reader = DynamicToDataReaderDataTableHelper<T>.ExportAsDataReader(enumerable);
            this.DoBulkCopy(reader, connection, transaction, tableName);
        }

        #endregion Methods
    }
}
