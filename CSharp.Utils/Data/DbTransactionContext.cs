using System.Data;

namespace CSharp.Utils.Data
{
    public sealed class DbTransactionContext : AbstractDisposable
    {
        #region Fields

        private IDbTransaction _transaction;

        #endregion Fields

        #region Constructors and Finalizers

        public DbTransactionContext(IDbConnection connection)
        {
            this.Connection = connection;
            this._transaction = connection.BeginTransaction();
            this.IsCommitted = false;
        }

        ~DbTransactionContext()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public IDbConnection Connection { get; private set; }

        public bool IsCommitted { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static DbTransactionContext CreateNew()
        {
            IDbConnection connection = DbConnectionFactory.Instance.CreateConnection();
            return new DbTransactionContext(connection);
        }

        public void Commit()
        {
            this.CheckAndThrowDisposedException();
            this._transaction.Commit();
            this.IsCommitted = true;
        }

        public IDbCommand CreateCommand()
        {
            this.CheckAndThrowDisposedException();
            IDbCommand command = this.Connection.CreateCommand();
            command.Transaction = this._transaction;
            return command;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (!this.IsCommitted)
            {
                this._transaction.Rollback();
            }

            this._transaction.Dispose();
            this.Connection.Close();
            this._transaction = null;
            this.Connection = null;
        }

        #endregion Methods
    }
}
