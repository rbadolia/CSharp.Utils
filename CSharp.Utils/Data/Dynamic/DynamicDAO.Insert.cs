using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        #region Public Methods and Operators

        public static int Insert(IEnumerable<E> entities, string tableName = null)
        {
            IDbCommand command = getCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Created, tableName);
            return executeTransactionCommand(command, entities, true, method);
        }

        public static int Insert(IEnumerable<E> entities, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Created, tableName);
            return executeCommand(command, entities, true, method);
        }

        public static int Insert(IEnumerable<E> entities, IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Created, tableName);
            return executeCommand(command, entities, true, method);
        }

        public static int Insert(E entity)
        {
            IDbConnection connection = getConnection();
            using (connection)
            {
                return Insert(entity, connection);
            }
        }

        public static int Insert(E entity, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Created, tableName);
            return updateInsertDelete(entity, command, true, method);
        }

        public static int Insert(E entity, IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Created, tableName);
            return updateInsertDelete(entity, command, true, method);
        }

        #endregion Public Methods and Operators
    }
}
