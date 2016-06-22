namespace CSharp.Utils.Data
{
    public interface IDbCallbackStrategy<in T>
    {
        #region Public Properties

        AfterExecuteDelegate<T> AfterExecute { get; }

        BeforeExecuteDelegate<T> BeforeExecute { get; }

        bool ExecuteAsReader { get; }

        InitializeCommandDelegate InitializeCommand { get; }

        #endregion Public Properties
    }
}
