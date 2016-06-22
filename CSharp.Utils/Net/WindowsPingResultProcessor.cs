using System;

namespace CSharp.Utils.Net
{
    public class WindowsPingResultProcessor : AbstractPingResultProcessor
    {
        #region Public Methods and Operators

        public override decimal ProcessPingResult(string pingOutput, string commandPrompt)
        {
            if (pingOutput != null)
            {
                if (pingOutput.IndexOf("Destination net unreachable", StringComparison.Ordinal) > -1)
                {
                    return 100;
                }

                int x = pingOutput.IndexOf("% loss)", StringComparison.Ordinal);
                if (x != -1)
                {
                    int y = pingOutput.LastIndexOf("(", x, StringComparison.Ordinal);
                    string result = pingOutput.Substring(y + 1, x - y - 1);
                    return Convert.ToDecimal(result);
                }
            }

            return 100;
        }

        #endregion Public Methods and Operators
    }
}
