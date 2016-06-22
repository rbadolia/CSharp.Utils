namespace CSharp.Utils.Contracts
{
    public interface IComponentController
    {
        #region Public Properties

        ControllableComponentState State { get; }

        bool SupportsPauseAndResume { get; }

        #endregion Public Properties

        #region Public Methods and Operators

        void PerformControllableAction(ControllableAction action);

        #endregion Public Methods and Operators
    }
}
