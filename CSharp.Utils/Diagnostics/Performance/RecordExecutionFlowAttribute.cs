using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class RecordExecutionFlowAttribute : Attribute
    {
        #region Constructors and Finalizers

        public RecordExecutionFlowAttribute(bool ignoreArguments)
        {
            this.IgnoreArguments = ignoreArguments;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool IgnoreArguments { get; private set; }

        #endregion Public Properties
    }
}
