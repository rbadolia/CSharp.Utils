using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CSharp.Utils.Data.Dynamic;
using CSharp.Utils.Data.Helpers;

namespace CSharp.Utils.Data
{
    public class DAO
    {
        #region Public Methods and Operators

        public static int Delete(IEnumerable entities)
        {
            int sum = 0;
            foreach (object entity in entities)
            {
                sum += Delete(entity);
            }

            return sum;
        }

        public static int Delete(object entity)
        {
            return (int)invokeMethod(entity, "Delete");
        }

        public static int Insert(IEnumerable entities)
        {
            int sum = 0;
            foreach (object entity in entities)
            {
                sum += Insert(entity);
            }

            return sum;
        }

        public static int Insert(object entity)
        {
            return (int)invokeMethod(entity, "Create");
        }

        public static List<T> Select<T>(string selectStatement)
        {
            return Select<T>(selectStatement, DbConnectionFactory.Instance.ConnectionClassType, DbConnectionFactory.Instance.ConnectionString);
        }

        public static List<T> Select<T>(string selectStatement, Type connectionType, string connectionString)
        {
            IDbConnection connection = DbConnectionFactory.Instance.CreateConnection(connectionType, connectionString);
            if (connection != null)
            {
                using (connection)
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = selectStatement;
                    connection.Open();
                    IDataReader reader = command.ExecuteReader();
                    var list = new List<T>();
                    while (reader.Read())
                    {
                        list.Add((T)reader[0]);
                    }

                    reader.Close();
                    return list;
                }
            }

            return new List<T>();
        }

        public static DataTable SelectAsDataTable(string selectStatement)
        {
            return SelectAsDataTable(selectStatement, DbConnectionFactory.Instance.ConnectionClassType, DbConnectionFactory.Instance.ConnectionString);
        }

        public static DataTable SelectAsDataTable(string selectStatement, Type connectionType, string connectionString)
        {
            IDbConnection connection = DbConnectionFactory.Instance.CreateConnection(connectionType, connectionString);
            using (connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = selectStatement;
                connection.Open();
                IDataReader reader = command.ExecuteReader();
                return reader.ExportAsDataTable("data");
            }
        }

        public static int Update(IEnumerable entities)
        {
            int sum = 0;
            foreach (object entity in entities)
            {
                sum += Update(entity);
            }

            return sum;
        }

        public static int Update(object entity)
        {
            return (int)invokeMethod(entity, "Update");
        }

        public static int UpdateOrInsert(IEnumerable entities)
        {
            int sum = 0;
            foreach (object entity in entities)
            {
                sum += UpdateOrInsert(entity);
            }

            return sum;
        }

        public static int UpdateOrInsert(object entity)
        {
            return (int)invokeMethod(entity, "UpdateOrInsert");
        }

        #endregion Public Methods and Operators

        #region Methods

        private static object invokeMethod(object entity, string methodName)
        {
            Type entityType = entity.GetType();
            Type dynamicDaoType = typeof(DynamicDAO<>).MakeGenericType(entityType);
            MethodInfo method = dynamicDaoType.GetMethod(methodName, new[] { entityType });
            return method.Invoke(null, new[] { entity });
        }

        #endregion Methods
    }
}
