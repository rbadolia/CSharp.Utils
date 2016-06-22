using System.Collections.Generic;
using CSharp.Utils.Data.Helpers;

namespace CSharp.Utils.Data
{
    public class GenericDbDataSink<T> : IDbDataSink<T>
    {
        #region Fields

        private readonly IDbCallbackStrategy<T> _callbackStrategy;

        private DbTransactionContext _context;

        #endregion Fields

        #region Constructors and Finalizers

        public GenericDbDataSink(IDbCallbackStrategy<T> callbackStrategy)
        {
            this._callbackStrategy = callbackStrategy;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public virtual void InsertOrUpdate(IEnumerable<T> dataTransferObjects, object tag = null)
        {
            DatabaseHelper.Execute(dataTransferObjects, this._callbackStrategy, this._context, tag);
        }

        public void SetDbTransactionContext(DbTransactionContext context)
        {
            this._context = context;
        }

        #endregion Public Methods and Operators
    }
}
