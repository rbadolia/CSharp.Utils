using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDummyConnection : AbstractDisposable, IDbConnection
    {
        #region Public Properties

        public string ConnectionString { get; set; }

        public int ConnectionTimeout { get; set; }

        public string Database
        {
            get
            {
                return string.Empty;
            }
        }

        public ConnectionState State
        {
            get
            {
                return ConnectionState.Open;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return null;
        }

        public IDbTransaction BeginTransaction()
        {
            return null;
        }

        public void ChangeDatabase(string databaseName)
        {
        }

        public void Close()
        {
        }

        public abstract IDbCommand CreateCommand();

        public void Open()
        {
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
        }

        #endregion Methods
    }
}
