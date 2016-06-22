namespace CSharp.Utils.Contracts
{
    public interface IControllable : IUnique
    {
        #region Public Properties

        IComponentController Controller { get; }

        bool IsRunning { get; }
        #endregion Public Properties
    }
}
