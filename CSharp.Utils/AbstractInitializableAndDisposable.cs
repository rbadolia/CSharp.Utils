using System;
using System.Xml.Serialization;
using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    [Serializable]
    public abstract class AbstractInitializableAndDisposable : AbstractDisposable, IInitializable
    {
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
                throw new ObjectAlreadyInitializedException(AbstractInitializable.ObjectAlreadyInitializedExceptionMessage);
            }
        }

        protected abstract void InitializeProtected();

        #endregion Methods
    }
}
