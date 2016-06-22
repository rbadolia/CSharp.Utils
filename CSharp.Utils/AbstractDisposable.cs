using System;
using System.Runtime.ConstrainedExecution;
using System.Xml.Serialization;

namespace CSharp.Utils
{
    [Serializable]
    public abstract class AbstractDisposable : CriticalFinalizerObject, IDisposable
    {
        #region Constants

        private const string ObjectDisposedExceptionMessage = "Object already disposed.";

        #endregion Constants

        #region Fields

        private readonly object syncLock = new object();

        #endregion Fields

        #region Public Properties

        [XmlIgnore]
        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                lock (this.syncLock)
                {
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.Dispose(true);
                        }
                        finally
                        {
                            this.IsDisposed = true;
                            GC.SuppressFinalize(this);
                        }
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected abstract void Dispose(bool disposing);

        protected void CheckAndThrowDisposedException()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(ObjectDisposedExceptionMessage);
            }
        }

        #endregion Methods
    }
}
