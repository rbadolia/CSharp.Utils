using System;

namespace CSharp.Utils.Net
{
    public class UnixPingResultProcessor : AbstractPingResultProcessor
    {
        #region Public Methods and Operators

        public override decimal ProcessPingResult(string pingOutput, string commandPrompt)
        {
            if (pingOutput != null)
            {
                int endIndex = pingOutput.IndexOf("% packet loss", StringComparison.Ordinal);
                if (endIndex > -1)
                {
                    int startIndex = endIndex - 1;
                    while (pingOutput[startIndex] != ' ')
                    {
                        startIndex--;
                    }

                    startIndex++;
                    string percentPacketLossStr = pingOutput.Substring(startIndex, endIndex - startIndex);
                    decimal percentPacketLoss = Convert.ToDecimal(percentPacketLossStr);
                    return percentPacketLoss;
                }
            }

            return 100;
        }

        #endregion Public Methods and Operators
    }
}
