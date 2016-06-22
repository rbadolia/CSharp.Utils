namespace CSharp.Utils.RequestProcessing
{
    public interface IMetricsEnabledRequestHandler<in TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        #region Public Methods and Operators

        RequestHandlerMetrics GetMetrics();

        #endregion Public Methods and Operators
    }
}
