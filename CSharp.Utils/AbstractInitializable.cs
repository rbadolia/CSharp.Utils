using System;
using System.Runtime.ConstrainedExecution;
using System.Xml.Serialization;
using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    [Serializable]
    public abstract class AbstractInitializable : CriticalFinalizerObject, IInitializable
    {
        #region Static Fields

        public static string ObjectAlreadyInitializedExceptionMessage = "Object already initialized. This operation can not performed after the abject is initialized.";

        #endregion Static Fields

        #region Fields

        private readonly object syncLock = new object();

        #endregion Fields

        #region Public Properties

        [XmlIgnore]
        public bool IsInitialized { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                lock (this.syncLock)
                {
                    if (!this.IsInitialized)
                    {
                        try
                        {
                            this.InitializeProtected();
                        }
                        finally
                        {
                            this.IsInitialized = true;
                        }
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected void CheckAndThrowObjectAlreadyInitializedException()
        {
            if (this.IsInitialized)
            {
                throw new ObjectAlreadyInitializedException(ObjectAlreadyInitializedExceptionMessage);
            }
        }

        protected abstract void InitializeProtected();

        #endregion Methods
    }
}
