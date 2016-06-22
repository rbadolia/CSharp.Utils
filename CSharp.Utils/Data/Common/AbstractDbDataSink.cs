using System.Collections.Generic;
using System.Data;
using CSharp.Utils.Data.Helpers;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDbDataSink<T> : IDbDataSink<T>
    {
        #region Constructors and Finalizers

        protected AbstractDbDataSink()
        {
            this.ExecuteAsReader = false;
        }

        #endregion Constructors and Finalizers

        #region Properties

        protected bool ExecuteAsReader { get; set; }

        protected DbTransactionContext TransactionContext { get; private set; }

        #endregion Properties

        #region Public Methods and Operators

        public virtual void InsertOrUpdate(IEnumerable<T> dataTransferObjects, object tag = null)
        {
            DatabaseHelper.Execute(dataTransferObjects, this.ExecuteAsReader, this.InitializeCommand, this.BeforeExecute, this.AfterExecute, this.TransactionContext, tag);
        }

        public void SetDbTransactionContext(DbTransactionContext context)
        {
            this.TransactionContext = context;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual void AfterExecute(IDbCommand command, T dataTransferObject, object returnedValue, object tag)
        {
        }

        protected abstract void BeforeExecute(IDbCommand command, T dataTransferObject, object tag);

        protected abstract void InitializeCommand(IDbCommand command, object tag);

        #endregion Methods
    }
}
