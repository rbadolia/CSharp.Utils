using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Xml;
using CSharp.Utils.Xml;

namespace CSharp.Utils.Data.Helpers
{
    public static class DatabaseHelper
    {
        public const string DEFAULT_CONNECTION_STRING_NAME_APP_KEY = "DefaultConnectionStringName";

        #region Public Methods and Operators

        public static IDbDataParameter AddParameter(IDbCommand command, string parameterName, object parameterValue = null, DbType databaseDataType = DbType.String, ParameterDirection direction = ParameterDirection.Input, int? size = null)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = databaseDataType;
            parameter.Direction = direction;
            if (size != null)
            {
                parameter.Size = size.Value;
            }

            if (parameterValue != null)
            {
                parameter.Value = parameterValue;
            }

            command.Parameters.Add(parameter);
            return parameter;
        }

        public static void BatchExecute<T>(IDbCommand command, IEnumerable<T> enumerable, bool executeAsReader, BeforeExecuteDelegate<T> beforeExecute, AfterExecuteDelegate<T> afterExecute = null, object tag = null)
        {
            foreach (T obj in enumerable)
            {
                try
                {
                    beforeExecute(command, obj, tag);
                    object returnedValue = executeAsReader ? command.ExecuteReader() : command.ExecuteScalar();
                    if (afterExecute != null)
                    {
                        afterExecute(command, obj, returnedValue, tag);
                    }
                }
                catch (Exception ex)
                {
                    throw new DbCommandExecuteException("Exception on executing DB command.T:" + typeof(T) + "\r\n" + ex.Message, ex, obj);
                }
            }
        }

        public static void Execute<T>(IEnumerable<T> dataTransferObjects, IDbCallbackStrategy<T> callbackStrategy, DbTransactionContext context = null, object tag = null)
        {
            Execute(dataTransferObjects, callbackStrategy.ExecuteAsReader, callbackStrategy.InitializeCommand, callbackStrategy.BeforeExecute, callbackStrategy.AfterExecute, context, tag);
        }

        public static void Execute<T>(IEnumerable<T> dataTransferObjects, bool executeAsReader, InitializeCommandDelegate initializeCommand, BeforeExecuteDelegate<T> beforeExecute, AfterExecuteDelegate<T> afterExecute = null, DbTransactionContext context = null, object tag = null)
        {
            IDbCommand command = context != null ? context.CreateCommand() : DbConnectionFactory.Instance.CreateConnection().CreateCommand();
            try
            {
                using (command)
                {
                    initializeCommand(command, tag);
                    try
                    {
                        BatchExecute(command, dataTransferObjects, executeAsReader, beforeExecute, afterExecute, tag);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
            }
            finally
            {
                if (context == null)
                {
                    command.Connection.Close();
                }
            }
        }

        public static List<RdbmsInfo> GetRdbmsInfos(XmlNode node)
        {
            var list = new List<RdbmsInfo>();
            foreach (XmlNode rdbmsNode in XmlHelper.GetNodes(node, "RdbmsInfos/RdbmsInfo"))
            {
                string id = XmlHelper.GetMandatoryStringAttribute(rdbmsNode, "id");
                string connectionTypeName = XmlHelper.GetMandatoryStringAttribute(rdbmsNode, "connectionType");
                Type connectionType = Type.GetType(connectionTypeName);
                string bulkCopyTypeName = XmlHelper.GetStringAttribute(rdbmsNode, "bulkCopyType");
                Type bulkCopyType = string.IsNullOrEmpty(bulkCopyTypeName) ? null : Type.GetType(bulkCopyTypeName);
                string defaultConnectionString = XmlHelper.GetStringAttribute(rdbmsNode, "defaultConnectionString");
                string dummyConnectionTypeName = XmlHelper.GetStringAttribute(rdbmsNode, "dummyConnectionType");
                Type dummyConnectionType = string.IsNullOrEmpty(dummyConnectionTypeName) ? null : Type.GetType(dummyConnectionTypeName);
                list.Add(new RdbmsInfo(id, connectionType, bulkCopyType, defaultConnectionString, dummyConnectionType));
            }

            return list;
        }

        public static string GetConnectionStringByName(string connectionStringName, bool getDefaultIfDoenNotExist)
        {
            if (!string.IsNullOrWhiteSpace(connectionStringName))
            {
                var connectionString = GetConnectionStringByName(connectionStringName);
                if (connectionString != null)
                {
                    return connectionString;
                }
            }

            if (!getDefaultIfDoenNotExist)
            {
                return null;
            }

            connectionStringName = ConfigurationManager.AppSettings[DEFAULT_CONNECTION_STRING_NAME_APP_KEY];
            return GetConnectionStringByName(connectionStringName);
        }

        private static string GetConnectionStringByName(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings != null)
            {
                return settings.ConnectionString;
            }

            return null;
        }

        #endregion Public Methods and Operators
    }
}
