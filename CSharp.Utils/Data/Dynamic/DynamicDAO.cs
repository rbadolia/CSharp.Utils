using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
        where E : class, new()
    {
        #region Static Fields

        private static readonly MetaData _metaData;

        private static List<string> _allColumnNames = new List<string>();

        private static string _deleteCommand;

        private static string _deleteCommandFormat;

        private static string _deleteCommandIfNoKey;

        private static string _deleteCommandIfNoKeyFormat;

        private static string _insertCommand;

        private static string _insertCommandFormat;

        private static List<string> _keyColumnNames = new List<string>();

        private static string _selectCommand;

        private static string _selectCommandFormat;

        private static string _selectOneCommand;

        private static string _selectOneCommandFormat;

        private static string _updateCommand;

        private static string _updateCommandFormat;

        #endregion Static Fields

        #region Public Properties

        public static string ConnectionString { get; set; }

        public static Type ConnectionType { get; set; }

        public static string InsertStatement { get; private set; }

        public static ICollection<string> KeyColumnNames
        {
            get
            {
                var list = new List<string>(_keyColumnNames);
                return list;
            }
        }

        public static string SelectStatement { get; private set; }

        public static string TableName { get; private set; }

        public static string UpdateStatement { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static IDataReader WrapInsideDataReader(IEnumerable<E> entities)
        {
            var columns = new List<KeyValuePair<string, Type>>();
            for (int i = 0; i < _allColumnNames.Count; i++)
            {
                columns.Add(new KeyValuePair<string, Type>(_allColumnNames[i], i < _metaData.KeyProperties.Count ? _metaData.KeyProperties[i].Key.PropertyType : _metaData.NonKeyProperties[i - _metaData.KeyProperties.Count].Key.PropertyType));
            }

            var strategy = new EnumeratorBasedDataReaderStrategy<E>(entities.GetEnumerator(), columns, populatePropertyValuesToArray);
            strategy.Initialize();
            var reader = new GenericDataReader(strategy);
            return reader;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void addKeyParametersForSelectOneOrDelete(IDbCommand command, params object[] keys)
        {
            for (int i = 0; i < _keyColumnNames.Count; i++)
            {
                addParameter(command, _keyColumnNames[i], keys[i]);
            }
        }

        private static IDbDataParameter addParameter(IDbCommand command, string parameterName, object value)
        {
            IDbDataParameter parameter = createParameter(command, parameterName);
            parameter.Value = value;
            return parameter;
        }

        private static E createEntityFromRecord(IDataRecord record)
        {
            var entity = new E();

            _populateEntityFromRecord(record, entity);
            return entity;
        }

        private static IDbDataParameter createParameter(IDbCommand command, string parameterName)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            command.Parameters.Add(parameter);
            return parameter;
        }

        private static int executeCommand(IDbCommand command, bool isNonQuery)
        {
            return isNonQuery ? command.ExecuteNonQuery() : Convert.ToInt32(command.ExecuteScalar());
        }

        private static int executeCommand(IDbCommand command, IEnumerable<E> entities, bool isInsert, populateValuesDelegate method)
        {
            int recordsAffected = entities.Sum(entity => updateInsertDelete(entity, command, isInsert, method));
            return recordsAffected;
        }

        private static int executeCommandAndCloseConnection(IDbCommand command, bool isNonQuery)
        {
            using (command.Connection)
            {
                return executeCommand(command, isNonQuery);
            }
        }

        private static int executeTransactionCommand(IDbCommand command, IEnumerable<E> entities, bool isInsert, populateValuesDelegate method)
        {
            using (command.Connection)
            {
                IDbTransaction transaction = command.Connection.BeginTransaction();
                command.Transaction = transaction;
                try
                {
                    int recordsAffected = entities.Sum(entity => updateInsertDelete(entity, command, isInsert, method));
                    transaction.Commit();
                    return recordsAffected;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static IDbCommand getCommand()
        {
            IDbConnection connection = getConnection();
            IDbCommand command = connection.CreateCommand();
            return command;
        }

        private static IDbConnection getConnection()
        {
            IDbConnection connection = DbConnectionFactory.Instance.CreateConnection();
            return connection;
        }

        private static populateValuesDelegate initializeCommandForDml(IDbCommand command, CudOperationType dmlOperationType, string tableName)
        {
            command.Parameters.Clear();
            bool isInsert = false;
            populateValuesDelegate method = null;
            switch (dmlOperationType)
            {
                case CudOperationType.Deleted:
                    command.CommandText = tableName == null ? _deleteCommandIfNoKey : string.Format(_deleteCommandIfNoKeyFormat, tableName);
                    method = _populateValuesFromEntityForUpdate;
                    break;

                case CudOperationType.Updated:
                    command.CommandText = tableName == null ? _updateCommand : string.Format(_updateCommandFormat, tableName);
                    method = _populateValuesFromEntityForUpdate;
                    break;

                case CudOperationType.Created:
                    command.CommandText = tableName == null ? _insertCommand : string.Format(_insertCommandFormat, tableName);
                    method = _populateValuesFromEntityForInsert;
                    isInsert = true;
                    break;
            }

            int j = isInsert ? 0 : _keyColumnNames.Count;
            for (int i = 0; i < _allColumnNames.Count; i++)
            {
                createParameter(command, _allColumnNames[(i + j) % _allColumnNames.Count]);
            }

            return method;
        }

        private static void initializeCommandForSelectOne(IDbCommand command, string tableName, params object[] keys)
        {
            command.Parameters.Clear();
            command.CommandText = tableName == null ? _selectOneCommand : string.Format(_selectOneCommandFormat, tableName);
            addKeyParametersForSelectOneOrDelete(command, keys);
        }

        private static void initializeCommandWithWhereClause(IDbCommand command, string whereClause, IList<ClauseCondition> whereClauseConditions)
        {
            if (string.IsNullOrWhiteSpace(whereClause) && (whereClauseConditions == null || whereClauseConditions.Count == 0))
            {
                return;
            }

            command.CommandText += " WHERE " + whereClause;
            if (whereClauseConditions != null)
            {
                for (int i = 0; i < whereClauseConditions.Count; i++)
                {
                    command.CommandText += whereClauseConditions[i].ParameterName + whereClauseConditions[i].Operator + "@" + whereClauseConditions[i].ParameterName;
                    if (i < whereClauseConditions.Count - 1)
                    {
                        command.CommandText += " AND ";
                    }

                    addParameter(command, whereClauseConditions[i].ParameterName, whereClauseConditions[i].ParameterValue);
                }
            }
        }

        private static void populatePropertyValuesToArray(E entity, object[] array)
        {
            _populateValuesFromEntityForInsert(array, entity);
        }

        private static int updateInsertDelete(E entity, IDbCommand command, bool isInsert, populateValuesDelegate method)
        {
            var values = new object[_allColumnNames.Count];
            method(values, entity);
            int j = isInsert ? 0 : _keyColumnNames.Count;
            updateParameters(command, values);
            int recordsAffected = executeCommand(command, false);
            return recordsAffected;
        }

        private static int updateOrInsert(E entity, IDbCommand command, string tableName = null)
        {
            populateValuesDelegate method = initializeCommandForDml(command, CudOperationType.Updated, tableName);
            int recordsAffected = updateInsertDelete(entity, command, false, method);
            if (recordsAffected == 0)
            {
                method = initializeCommandForDml(command, CudOperationType.Created, tableName);
                updateInsertDelete(entity, command, true, method);
                recordsAffected = 0;
            }

            return recordsAffected;
        }

        private static void updateParameters(IDbCommand command, IList<object> values)
        {
            for (int i = 0; i < _allColumnNames.Count; i++)
            {
                var parameter = (IDbDataParameter)command.Parameters[i];
                parameter.Value = values[i] ?? DBNull.Value;
            }
        }

        #endregion Methods
    }
}
