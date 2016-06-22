using System;
using CSharp.Utils.Diagnostics;

namespace CSharp.Utils.RequestProcessing
{
    public class MetricsEnabledRequestHandlerDecorator<TRequest> : AbstractRequestHandler<TRequest>, IMetricsEnabledRequestHandler<TRequest>
        where TRequest : IRequest
    {
        #region Fields

        private readonly IRequestHandler<TRequest> decoratedRequestHandler;

        private readonly RequestHandlerMetrics metrics;

        #endregion Fields

        #region Constructors and Finalizers

        public MetricsEnabledRequestHandlerDecorator(IRequestHandler<TRequest> decoratedRequestHandler)
        {
            this.decoratedRequestHandler = decoratedRequestHandler;
            this.metrics = new RequestHandlerMetrics();
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public RequestHandlerMetrics GetMetrics()
        {
            return this.metrics.Clone() as RequestHandlerMetrics;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
        }

        protected override void InitializeProtected()
        {
        }

        protected override IResponse processRequestProtected(TRequest request)
        {
            this.metrics.RequestReceived();
            long ticksBefore = SharedStopWatch.ElapsedTicks;
            IResponse response = null;
            try
            {
                response = this.decoratedRequestHandler.ProcessRequest(request);
            }
            finally
            {
                long ticksTaken = SharedStopWatch.ElapsedTicks - ticksBefore;
                this.metrics.RequestProcessed(ticksTaken, response == null || response.Result == ProcessingResult.Fail);
                if (response != null)
                {
                    var metricsEnabledResponse = response as IMetricsEnabledResponse;
                    if (metricsEnabledResponse != null)
                    {
                        metricsEnabledResponse.TimeTaken = TimeSpan.FromTicks(ticksTaken);
                    }
                }
            }

            return response;
        }

        #endregion Methods
    }
}
