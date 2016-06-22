using System;
using System.Collections;
using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDbStore
    {
        #region Public Properties

        public string ConnectionString { get; set; }

        public IDbActivityStrategy DbActivityStrategy { get; set; }

        public IDbStrategy DbStrategy { get; set; }

        public string TableName { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Delete(IEnumerable messages, object tag)
        {
            IDbCommand command = this.GetCommand();
            using (command.Connection)
            {
                foreach (object message in messages)
                {
                    this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Deleted, message);
                    this.ExecuteCommand(command, false);
                }
            }
        }

        public int Delete(object message, object tag)
        {
            IDbCommand command = this.GetCommand();
            this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Deleted, message);
            int recordsAffected = this.ExecuteCommandAndCloseConnection(command, false);
            return recordsAffected;
        }

        public IEnumerable Select()
        {
            return this.@select(null);
        }

        public IEnumerable Select(string clause)
        {
            return this.@select(clause);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual IDbCommand GetCommand()
        {
            IDbConnection connection = this.DbStrategy.CreateConnection();
            connection.ConnectionString = this.ConnectionString;
            IDbCommand command = connection.CreateCommand();
            return command;
        }

        protected void Insert(IEnumerable messages)
        {
            IDbCommand command = this.GetCommand();
            using (command.Connection)
            {
                this.Insert(messages, command);
            }
        }

        protected void Insert(object message)
        {
            IDbCommand command = this.GetCommand();
            using (command.Connection)
            {
                this.Insert(message, command);
            }
        }

        protected void UpdateOrInsert(IEnumerable messages)
        {
            IDbCommand command = this.GetCommand();
            using (command.Connection)
            {
                this.UpdateOrInsert(messages, command);
            }
        }

        protected void UpdateOrInsert(object message)
        {
            IDbCommand command = this.GetCommand();
            using (command.Connection)
            {
                this.UpdateOrInsert(message, command);
            }
        }

        protected virtual void Insert(object message, IDbCommand command)
        {
            command.Parameters.Clear();
            this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Created, message);
            this.ExecuteCommand(command, true);
        }

        protected virtual int UpdateOrInsert(object message, IDbCommand command)
        {
            command.Parameters.Clear();
            this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Updated, message);
            int recordsAffected = this.ExecuteCommand(command, false);
            if (recordsAffected == 0)
            {
                this.Insert(message, command);
                recordsAffected = 0;
            }

            return recordsAffected;
        }

        private int ExecuteCommand(IDbCommand command, bool isNonQuery)
        {
            if (isNonQuery)
            {
                command.ExecuteNonQuery();
                return -1;
            }

            int recordsAffected = Convert.ToInt32(command.ExecuteScalar());
            return recordsAffected;
        }

        private int ExecuteCommandAndCloseConnection(IDbCommand command, bool isNonQuery)
        {
            using (command.Connection)
            {
                return this.ExecuteCommand(command, isNonQuery);
            }
        }

        private void Insert(IEnumerable messages, IDbCommand command)
        {
            using (command.Connection)
            {
                foreach (object message in messages)
                {
                    this.Insert(message, command);
                }
            }
        }

        private IEnumerable select(string clause)
        {
            IDbCommand command = this.GetCommand();
            this.DbActivityStrategy.InitializeCommandForSelect(command, clause);
            IDataReader reader = command.ExecuteReader();
            return new DataReaderEnumerable<object>(reader, this.DbActivityStrategy.CreateMessageFromRecord, command.Connection, true);
        }

        private int UpdateOrInsert(IEnumerable messages, IDbCommand command)
        {
            int sum = 0;
            foreach (object message in messages)
            {
                sum += this.UpdateOrInsert(message, command);
            }

            return sum;
        }

        #endregion Methods
    }
}
