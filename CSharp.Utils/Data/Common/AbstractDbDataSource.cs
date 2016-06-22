using System.Collections.Generic;
using System.Data;
using CSharp.Utils.Data.Helpers;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDbDataSource<T> : IDbDataSource<T>
    {
        #region Constructors and Finalizers

        protected AbstractDbDataSource()
        {
            this.ExecuteAsReader = false;
        }

        #endregion Constructors and Finalizers

        #region Properties

        protected bool ExecuteAsReader { get; set; }

        protected DbTransactionContext TransactionContext { get; private set; }

        #endregion Properties

        #region Public Methods and Operators

        public virtual void Populate(IEnumerable<T> dataTransferObjects, object tag = null)
        {
            DatabaseHelper.Execute(dataTransferObjects, this.ExecuteAsReader, this.initializeCommand, this.BeforeExecute, this.AfterExecute, this.TransactionContext, tag);
        }

        public void SetDbTransactionContext(DbTransactionContext context)
        {
            this.TransactionContext = context;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected abstract void AfterExecute(IDbCommand command, T dataTransferObject, object returnedValue, object tag);

        protected abstract void BeforeExecute(IDbCommand command, T dataTransferObject, object tag);

        protected abstract void initializeCommand(IDbCommand command, object tag);

        #endregion Methods
    }
}
