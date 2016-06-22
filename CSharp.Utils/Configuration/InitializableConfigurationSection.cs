using System.Configuration;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.Configuration
{
    public abstract class InitializableConfigurationSection : ConfigurationSection, IInitializable
    {
        #region Public Properties

        public bool IsInitialized { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.InitializeProtected();
                this.IsInitialized = true;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected abstract void InitializeProtected();

        #endregion Methods
    }
}
