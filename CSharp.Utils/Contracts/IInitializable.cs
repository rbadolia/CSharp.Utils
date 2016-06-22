namespace CSharp.Utils.Contracts
{
    public interface IInitializable
    {
        #region Public Properties

        bool IsInitialized { get; }

        #endregion Public Properties

        #region Public Methods and Operators

        void Initialize();

        #endregion Public Methods and Operators
    }
}
