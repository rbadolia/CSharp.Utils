using System;
using System.Threading;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Diagnostics.Performance
{
    [PerfCounterCategory("MethodExecutionMetrics")]
    public class MethodExecutionMetrics
    {
        #region Fields

        private long maximumTimeTakenInTicks = long.MinValue;

        private long minimumTimeTakenInTicks = long.MaxValue;

        private long numberOfExecutions;

        private long totalTimeTakenInTicks;

        #endregion Fields

        #region Public Properties

        [PerfCounter(CounterTimeTypes = TimeTypes.Milliseconds | TimeTypes.Seconds | TimeTypes.Minutes)]
        public TimeSpan AverageTimeTaken
        {
            get
            {
                return TimeSpan.FromTicks(this.numberOfExecutions == 0 ? 0 : (this.totalTimeTakenInTicks / this.numberOfExecutions));
            }
        }

        [PerfCounter]
        public string ClassName { get; set; }

        [PerfCounter(CounterTimeTypes = TimeTypes.Milliseconds | TimeTypes.Seconds | TimeTypes.Minutes)]
        public TimeSpan MaximumTimeTaken
        {
            get
            {
                return TimeSpan.FromTicks(this.maximumTimeTakenInTicks == long.MinValue ? 0 : this.maximumTimeTakenInTicks);
            }
        }

        [PerfCounter]
        public string MethodName { get; set; }

        [PerfCounter(CounterTimeTypes = TimeTypes.Milliseconds | TimeTypes.Seconds | TimeTypes.Minutes)]
        public TimeSpan MinimumTimeTaken
        {
            get
            {
                return TimeSpan.FromTicks(this.minimumTimeTakenInTicks == long.MaxValue ? 0 : this.minimumTimeTakenInTicks);
            }
        }

        [PerfCounter]
        public long NumberOfExecutions
        {
            get
            {
                return this.numberOfExecutions;
            }
        }

        [PerfCounter(CounterTimeTypes = TimeTypes.Milliseconds | TimeTypes.Seconds | TimeTypes.Minutes | TimeTypes.Hours)]
        public TimeSpan TotalTimeTaken
        {
            get
            {
                return TimeSpan.FromTicks(this.totalTimeTakenInTicks);
            }
        }

        [PerfCounter]
        public string UniqueMethodName { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Increment(long ticksTaken)
        {
            Interlocked.Increment(ref this.numberOfExecutions);
            Interlocked.Add(ref this.totalTimeTakenInTicks, ticksTaken);
            ThreadingHelper.ExchangeIfGreaterThan(ref this.maximumTimeTakenInTicks, ticksTaken);
            ThreadingHelper.ExchangeIfLessThan(ref this.minimumTimeTakenInTicks, ticksTaken);
        }

        #endregion Public Methods and Operators
    }
}
