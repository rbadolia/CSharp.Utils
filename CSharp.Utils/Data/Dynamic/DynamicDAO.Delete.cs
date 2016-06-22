using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        #region Public Methods and Operators

        public static int Delete(IEnumerable<E> entities, string tableName = null)
        {
            IDbCommand command = getCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeTransactionCommand(command, entities, false, method);
        }

        public static int Delete(IEnumerable<E> entities, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeCommand(command, entities, false, method);
        }

        public static int Delete(IEnumerable<E> entities, IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeCommand(command, entities, false, method);
        }

        public static int Delete(E entity, string tableName = null)
        {
            IDbCommand command = getCommand();
            initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeCommandAndCloseConnection(command, false);
        }

        public static int Delete(E entity, IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeCommand(command, false);
        }

        public static int Delete(E entity, IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            initializeCommandForDml(command, CudOperationType.Deleted, tableName);
            return executeCommand(command, false);
        }

        public static int Delete(string whereClause, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            IDbCommand command = getCommand();
            command.Parameters.Clear();
            command.CommandText = tableName == null ? _deleteCommand : string.Format(_deleteCommandFormat, tableName);
            initializeCommandWithWhereClause(command, whereClause, whereClauseConditions);
            return executeCommandAndCloseConnection(command, false);
        }

        public static int Delete(string whereClause, IDbConnection connection, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            IDbCommand command = connection.CreateCommand();
            command.Parameters.Clear();
            command.CommandText = tableName == null ? _deleteCommand : string.Format(_deleteCommandFormat, tableName);
            initializeCommandWithWhereClause(command, whereClause, whereClauseConditions);
            return executeCommand(command, false);
        }

        public static int Delete(string whereClause, IDbTransaction transaction, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            command.Parameters.Clear();
            command.CommandText = tableName == null ? _deleteCommand : string.Format(_deleteCommandFormat, tableName);
            initializeCommandWithWhereClause(command, whereClause, whereClauseConditions);
            return executeCommand(command, false);
        }

        public static int DeleteAll(string tableName = null)
        {
            IDbCommand command = getCommand();
            command.CommandText = "DELETE FROM " + (tableName ?? TableName);
            return executeCommandAndCloseConnection(command, false);
        }

        public static int DeleteAll(IDbConnection connection, string tableName = null)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM " + (tableName ?? TableName);
            return executeCommand(command, false);
        }

        public static int DeleteAll(IDbTransaction transaction, string tableName = null)
        {
            IDbCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM " + (tableName ?? TableName);
            return executeCommand(command, false);
        }

        #endregion Public Methods and Operators
    }
}
