using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class ExecutionFlowRecordedEventArgs : EventArgs
    {
        #region Constructors and Finalizers

        public ExecutionFlowRecordedEventArgs(ExecutionFlow executionFlow)
        {
            this.ExecutionFlow = executionFlow;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public ExecutionFlow ExecutionFlow { get; private set; }

        #endregion Public Properties
    }
}
