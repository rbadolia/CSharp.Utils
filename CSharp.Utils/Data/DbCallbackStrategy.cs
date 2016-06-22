namespace CSharp.Utils.Data
{
    public sealed class DbCallbackStrategy<T> : IDbCallbackStrategy<T>
    {
        #region Constructors and Finalizers

        public DbCallbackStrategy(bool executeAsReader, InitializeCommandDelegate initializeCommand, BeforeExecuteDelegate<T> beforeExecute, AfterExecuteDelegate<T> afterExecute = null)
        {
            this.ExecuteAsReader = executeAsReader;
            this.InitializeCommand = initializeCommand;
            this.BeforeExecute = beforeExecute;
            this.AfterExecute = afterExecute;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public AfterExecuteDelegate<T> AfterExecute { get; private set; }

        public BeforeExecuteDelegate<T> BeforeExecute { get; private set; }

        public bool ExecuteAsReader { get; private set; }

        public InitializeCommandDelegate InitializeCommand { get; private set; }

        #endregion Public Properties
    }
}
