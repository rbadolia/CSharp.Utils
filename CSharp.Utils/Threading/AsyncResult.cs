using System;
using System.Threading;

namespace CSharp.Utils.Threading
{
    public class AsyncResult<T> : AbstractDisposable, IAsyncResult
    {
        #region Fields

        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);

        #endregion Fields

        #region Constructors and Finalizers

        internal AsyncResult()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public object AsyncState
        {
            get
            {
                return this.TypedAsyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return this._waitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public Exception Exception { get; private set; }

        public bool IsCompleted { get; private set; }

        public T TypedAsyncState { get; internal set; }

        #endregion Public Properties

        #region Methods

        internal void SetCompleted(T returnedValue)
        {
            this.IsCompleted = true;
            this.TypedAsyncState = returnedValue;
            this._waitHandle.Set();
        }

        internal void SetCompletedWithException(Exception ex)
        {
            this.IsCompleted = true;
            this.Exception = ex;
            this._waitHandle.Set();
        }

        protected override void Dispose(bool disposing)
        {
            this.AsyncWaitHandle.Dispose();
        }

        #endregion Methods
    }
}
