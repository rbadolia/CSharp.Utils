using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        #region Public Methods and Operators

        public static IEnumerable<E> Select(string whereClause, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            IDbConnection connection = getConnection();
            return select(whereClause, connection, null, true, tableName, whereClauseConditions);
        }

        public static IEnumerable<E> Select(string whereClause, IDbConnection connection, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            return select(whereClause, connection, null, false, tableName, whereClauseConditions);
        }

        public static IEnumerable<E> Select(string whereClause, IDbTransaction transaction, string tableName = null, params ClauseCondition[] whereClauseConditions)
        {
            return select(whereClause, null, transaction, false, tableName, whereClauseConditions);
        }

        public static IEnumerable<E> SelectAll(string tableName = null)
        {
            IDbConnection connection = getConnection();
            return select(null, connection, null, true, tableName, null);
        }

        public static IEnumerable<E> SelectAll(IDbConnection connection, string tableName = null)
        {
            return Select(null, connection, tableName);
        }

        public static IEnumerable<E> SelectAll(IDbTransaction transaction, string tableName = null)
        {
            return Select(null, transaction, tableName);
        }

        public static E SelectOne(string tableName = null, params object[] keys)
        {
            IDbConnection connection = getConnection();
            using (connection)
            {
                return SelectOne(connection, tableName, keys);
            }
        }

        public static E SelectOne(IDbConnection connection, string tableName = null, params object[] keys)
        {
            IDbCommand command = connection.CreateCommand();
            initializeCommandForSelectOne(command, tableName, keys);
            IDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return createEntityFromRecord(reader);
            }

            return null;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static IEnumerable<E> select(string whereClause, IDbConnection connection, IDbTransaction transaction, bool closeConnectionAfterReading, string tableName, params ClauseCondition[] whereClauseConditions)
        {
            IDbCommand command;
            if (transaction != null)
            {
                command = transaction.Connection.CreateCommand();
                command.Transaction = transaction;
            }
            else
            {
                command = connection.CreateCommand();
            }

            command.CommandText = tableName == null ? _selectCommand : string.Format(_selectCommandFormat, tableName);
            initializeCommandWithWhereClause(command, whereClause, whereClauseConditions);
            IDataReader reader = command.ExecuteReader();
            return new DataReaderEnumerable<E>(reader, createEntityFromRecord, command.Connection, closeConnectionAfterReading);
        }

        #endregion Methods
    }
}
