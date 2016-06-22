using System;

namespace CSharp.Utils.Net
{
    public abstract class AbstractPingResultProcessor : IPingResultProcessor
    {
        #region Public Methods and Operators

        public abstract decimal ProcessPingResult(string pingResult, string commandPrompt);

        public virtual string RemovePadding(string output, string commandPrompt)
        {
            string result = output;
            if (!string.IsNullOrEmpty(output))
            {
                int ind = output.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (ind > -1)
                {
                    result = output.Substring(0, ind);
                }
            }

            return string.IsNullOrEmpty(result) ? " " : result;
        }

        #endregion Public Methods and Operators
    }
}
