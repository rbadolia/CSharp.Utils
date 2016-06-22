using System;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.RequestProcessing.Core;

namespace CSharp.Utils.RequestProcessing
{
    public abstract class AbstractRequestHandler<TRequest> : AbstractInitializableAndDisposable, IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        #region Public Methods and Operators

        public virtual IResponse ProcessRequest(TRequest request)
        {
            long ticksBefore = SharedStopWatch.ElapsedTicks;
            IResponse response = null;
            try
            {
                response = this.processRequestProtected(request);
            }
            finally
            {
                long ticksTaken = SharedStopWatch.ElapsedTicks - ticksBefore;
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

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IResponse IRequestHandler.ProcessRequest(IRequest request)
        {
            return this.ProcessRequest((TRequest)request);
        }

        #endregion Explicit Interface Methods

        #region Methods

        protected abstract IResponse processRequestProtected(TRequest request);

        #endregion Methods
    }
}
