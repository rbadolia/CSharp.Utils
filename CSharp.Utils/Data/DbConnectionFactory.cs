using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CSharp.Utils.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data
{
    public sealed class DbConnectionFactory : AbstractInitializable
    {
        #region Static Fields

        private static readonly DbConnectionFactory InstanceObject = new DbConnectionFactory();

        #endregion Static Fields

        #region Fields

        private readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncLock = new object();

        private Type _connectionClassType;

        private string _connectionString;

        #endregion Fields

        #region Constructors and Finalizers

        private DbConnectionFactory()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static DbConnectionFactory Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public Type ConnectionClassType
        {
            get
            {
                return this._connectionClassType;
            }

            set
            {
                ValidateConnectionClassType(value);
                this._connectionClassType = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this._connectionString;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(@"The value provided for ConnectionString is null or white space", "value");
                }

                this._connectionString = value;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public IDbConnection CreateConnection(string connectionString = null)
        {
            this.Initialize();
            IDbConnection connection = this.CreateConnectionPrivate(this.ConnectionClassType, connectionString);
            return connection;
        }

        public IDbConnection CreateConnection(Type connectionClassType, string connectionString = null)
        {
            this.Initialize();
            return this.CreateConnectionPrivate(connectionClassType, connectionString);
        }

        public IDbConnection CreateConnection<T>(string connectionString) where T : IDbConnection, new()
        {
            IDbConnection connection = this.CreateConnection(typeof(T), connectionString);
            return connection;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void InitializeProtected()
        {
            this.populateConnectionStrings();
            if (this._connectionClassType == null)
            {
                string connectionType = ConfigurationManager.AppSettings["ConnectionClassType"];
                if (!string.IsNullOrEmpty(connectionType))
                {
                    this.ConnectionClassType = ReflectionHelper.GetType(connectionType);
                }
                else
                {
                    this._connectionClassType = typeof(SqlConnection);
                }
            }

            if (this._connectionString == null)
            {
                this._connectionString = ConfigurationManager.AppSettings["DefaultConnectionString"];

                if (string.IsNullOrWhiteSpace(this._connectionString))
                {
                    throw new Exception("No default connection string is specified in the configuration.");
                }

                this._connectionString = this.getConnectionString(this._connectionString);
            }
        }

        private static void ValidateConnectionClassType(Type connectionClassType)
        {
            Guard.ArgumentNotNull(connectionClassType, "connectionClassType");
            if (!typeof(IDbConnection).IsAssignableFrom(connectionClassType))
            {
                throw new ArgumentException(@"The 'Type' provided for connectionClassType cannot be treated as IDbConnection", "connectionClassType");
            }

            if (connectionClassType.IsAbstract || connectionClassType.IsInterface)
            {
                throw new ArgumentException(@"The 'Type' provided for connectionClassType should be a non-static, non-abstract class type", "connectionClassType");
            }

            if (connectionClassType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException(@"The 'Type' provided for connectionClassType must have a default constructor", "connectionClassType");
            }
        }

        private IDbConnection CreateConnectionPrivate(Type connectionClassType, string connectionString)
        {
            connectionString = this.getConnectionString(connectionString);
            ValidateConnectionClassType(connectionClassType);
            var connection = (IDbConnection)Activator.CreateInstance(connectionClassType);
            connection.ConnectionString = connectionString;
            connection.Open();
            return connection;
        }

        private string getConnectionString(string connectionString)
        {
            if (connectionString == null)
            {
                return this.getConnectionString(this._connectionString);
            }

            string cs;
            if (this._connectionStrings.TryGetValue(connectionString, out cs))
            {
                return cs;
            }

            return connectionString;
        }

        private void populateConnectionStrings()
        {
            this._connectionStrings.Clear();
            foreach (ConnectionStringSettings settings in ConfigurationManager.ConnectionStrings)
            {
                this._connectionStrings.Add(settings.Name, settings.ConnectionString);
            }
        }

        #endregion Methods
    }
}
