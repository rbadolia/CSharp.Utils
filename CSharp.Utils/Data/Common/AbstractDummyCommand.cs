using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDummyCommand : AbstractDisposable, IDbCommand
    {
        #region Public Properties

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection Connection { get; set; }

        public IDataParameterCollection Parameters { get; protected set; }

        public IDbTransaction Transaction { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Cancel()
        {
        }

        public abstract IDbDataParameter CreateParameter();

        public int ExecuteNonQuery()
        {
            return 0;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return null;
        }

        public IDataReader ExecuteReader()
        {
            return null;
        }

        public object ExecuteScalar()
        {
            return null;
        }

        public void Prepare()
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
