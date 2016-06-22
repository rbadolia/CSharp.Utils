namespace CSharp.Utils.RequestProcessing.Core
{
    public interface IMetricsEnabledRequestHandler : IRequestHandler
    {
        #region Public Methods and Operators

        RequestHandlerMetrics GetMetrics();

        #endregion Public Methods and Operators
    }
}
