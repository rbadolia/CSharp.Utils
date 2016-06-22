using System;
using System.Collections.Generic;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Diagnostics.Performance.Internal;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public static class RecordExecutionFlowHelper
    {
        #region Static Fields

        [ThreadStatic]
        private static ExecutionFlowContainer _container;

        #endregion Static Fields

        #region Public Properties

        public static string ThreadObjectName
        {
            get
            {
                return "EXECUTION_FLOW";
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        [CautionUsedByReflection]
        public static ExecutionFlow MethodCalled(string className, string methodName, IList<Pair<string, object>> arguments)
        {
            bool isNew;
            ExecutionFlow flow = getFlow(className, methodName, arguments, out isNew);
            if (!isNew)
            {
                flow.MethodCalled(className, methodName, arguments);
            }

            return flow;
        }

        [CautionUsedByReflection]
        public static void MethodExecuted(ExecutionFlow flow, Exception ex)
        {
            flow.MethodExecuted(ex);
            if (flow.IsExecutionCompleted)
            {
                _container.flow = null;
                ExecutionFlowRecorder.Instance.RaiseFlowRecordedEvent(flow);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private static ExecutionFlow getFlow(string className, string methodName, IList<Pair<string, object>> arguments, out bool isNew)
        {
            isNew = false;
            if (_container == null)
            {
                _container = new ExecutionFlowContainer();
            }

            if (_container.flow == null)
            {
                isNew = true;
                _container.flow = new ExecutionFlow(className, methodName, arguments);
            }

            return _container.flow;
        }

        #endregion Methods
    }
}
