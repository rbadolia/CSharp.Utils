using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        #region Public Methods and Operators

        public static int Update(IEnumerable<E> entities, string tableName = null)
        {
            IDbCommand command = getCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Updated, tableName);
            return executeTransactionCommand(command, entities, false, method);
        }

        public static int Update(IEnumerable<E> entities, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Updated, tableName);
            return executeCommand(command, entities, false, method);
        }

        public static int Update(IEnumerable<E> entities, IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Updated, tableName);
            return executeCommand(command, entities, false, method);
        }

        public static int Update(E entity, string tableName = null)
        {
            IDbCommand command = getCommand();
            using (command.Connection)
            {
                return Update(entity, command, tableName);
            }
        }

        public static int Update(E entity, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            return Update(entity, command, tableName);
        }

        public static int Update(E entity, IDbCommand command, string tableName = null)
        {
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Updated, tableName);
            return updateInsertDelete(entity, command, false, method);
        }

        public static int UpdateOrInsert(IEnumerable<E> entities)
        {
            IDbConnection connection = getConnection();
            using (connection)
            {
                IDbTransaction transaction = connection.BeginTransaction();
                try
                {
                    int result = UpdateOrInsert(entities, transaction);
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static int UpdateOrInsert(IEnumerable<E> entities, IDbConnection connection)
        {
            IDbCommand command = connection.CreateCommand();
            int recordsAffected = entities.Sum(entity => updateOrInsert(entity, command));
            return recordsAffected;
        }

        public static int UpdateOrInsert(IEnumerable<E> entities, IDbTransaction transaction)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            int recordsAffected = entities.Sum(entity => updateOrInsert(entity, command));
            return recordsAffected;
        }

        public static int UpdateOrInsert(E entity)
        {
            IDbConnection connection = getConnection();
            using (connection)
            {
                return UpdateOrInsert(entity, connection);
            }
        }

        public static int UpdateOrInsert(E entity, IDbConnection connection)
        {
            IDbCommand command = connection.CreateCommand();
            return updateOrInsert(entity, command);
        }

        public static int UpdateOrInsert(E entity, IDbTransaction transaction)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            return updateOrInsert(entity, command);
        }

        #endregion Public Methods and Operators
    }
}
