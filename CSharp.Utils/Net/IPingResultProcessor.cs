namespace CSharp.Utils.Net
{
    public interface IPingResultProcessor
    {
        #region Public Methods and Operators

        decimal ProcessPingResult(string pingResult, string commandPrompt);

        string RemovePadding(string output, string commandPrompt);

        #endregion Public Methods and Operators
    }
}
