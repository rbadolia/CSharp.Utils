using System;
using System.Threading;
using CSharp.Utils.Diagnostics.Performance;
using CSharp.Utils.Reflection;
using CSharp.Utils.Threading;

namespace CSharp.Utils.RequestProcessing
{
    [Serializable]
    [PerfCounterCategory("Request Handler Metrics")]
    public class RequestHandlerMetrics : ICloneable
    {
        #region Fields

        private long maximumTicksTakenForProcessing;

        private long minimumTicksTakenForProcessing;

        private long numberOfRequestsFailed;

        private long numberOfRequestsInProgress;

        private long numberOfRequestsProcessed;

        private long numberOfRequestsReceived;

        private long numberOfRequestsIgnored;

        private long totalTicksTakenForProcessing;

        #endregion Fields

        #region Public Properties

        [PerfCounter]
        public TimeSpan AverageTimeTakenPerRequest
        {
            get
            {
                long n = this.numberOfRequestsProcessed;
                return TimeSpan.FromTicks(n == 0 ? 0 : this.totalTicksTakenForProcessing / n);
            }
        }

        [PerfCounter]
        public TimeSpan MaximumTimeTakenForProcessing
        {
            get
            {
                long value = this.maximumTicksTakenForProcessing;
                return TimeSpan.FromTicks(value == long.MinValue ? 0 : value);
            }
        }

        [PerfCounter]
        public TimeSpan MinimumTimeTakenForProcessing
        {
            get
            {
                long value = this.minimumTicksTakenForProcessing;
                return TimeSpan.FromTicks(value == long.MaxValue ? 0 : value);
            }
        }

        [PerfCounter]
        public long NumberOfRequestsFailed
        {
            get
            {
                return this.numberOfRequestsFailed;
            }
        }

        [PerfCounter]
        public long NumberOfRequestsInProgress
        {
            get
            {
                return this.numberOfRequestsInProgress;
            }
        }

        [PerfCounter]
        public long NumberOfRequestsProcessed
        {
            get
            {
                return this.numberOfRequestsProcessed;
            }
        }

        [PerfCounter]
        public long NumberOfRequestsIgnored
        {
            get
            {
                return this.numberOfRequestsIgnored;
            }
        }

        [PerfCounter]
        public long NumberOfRequestsReceived
        {
            get
            {
                return this.numberOfRequestsReceived;
            }
        }

        [PerfCounter]
        public TimeSpan TotalTimeTakenForProcessing
        {
            get
            {
                return TimeSpan.FromTicks(this.totalTicksTakenForProcessing);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public object Clone()
        {
            var obj = new RequestHandlerMetrics();
            this.CopyTo(obj);
            return obj;
        }

        public void CopyTo(RequestHandlerMetrics metrics)
        {
            DynamicCopyHelper<RequestHandlerMetrics, RequestHandlerMetrics>.Copy(this, metrics);
        }

        public void RequestProcessed(long ticksTaken, bool isFail)
        {
            Interlocked.Decrement(ref this.numberOfRequestsInProgress);
            Interlocked.Increment(ref this.numberOfRequestsProcessed);

            if (isFail)
            {
                Interlocked.Increment(ref this.numberOfRequestsFailed);
            }

            Interlocked.Add(ref this.totalTicksTakenForProcessing, ticksTaken);
            ThreadingHelper.ExchangeIfGreaterThan(ref this.maximumTicksTakenForProcessing, ticksTaken);
            ThreadingHelper.ExchangeIfLessThan(ref this.minimumTicksTakenForProcessing, ticksTaken);
        }

        public void RequestReceived()
        {
            Interlocked.Increment(ref this.numberOfRequestsReceived);
            Interlocked.Increment(ref this.numberOfRequestsInProgress);
        }

        public void RequestIgnored()
        {
            Interlocked.Decrement(ref this.numberOfRequestsInProgress);
            Interlocked.Increment(ref this.numberOfRequestsIgnored);
        }

        #endregion Public Methods and Operators
    }
}
