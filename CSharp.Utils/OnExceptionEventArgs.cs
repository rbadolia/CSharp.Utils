using System;

namespace CSharp.Utils
{
    public sealed class OnExceptionEventArgs : EventArgs
    {
        #region Constructors and Finalizers

        public OnExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public Exception Exception { get; private set; }

        #endregion Public Properties
    }
}
