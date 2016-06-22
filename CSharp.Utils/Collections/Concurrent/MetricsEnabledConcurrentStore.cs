using System;
using System.Threading;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Diagnostics.Performance;

namespace CSharp.Utils.Collections.Concurrent
{
    [PerfCounterCategory("Concurrent Stores")]
    public sealed class MetricsEnabledConcurrentStore<T> : IConcurrentStore<T>
    {
        #region Fields

        private readonly IConcurrentStore<T> store;

        private long maxTicksTakenForFlush;

        private long numberOfFlushes;

        private long numberOfItemsAttemptedToWrite;

        private long numberOfItemsFlushed;

        private long numberOfItemsSkipped;

        private long numberOfItemsWritten;

        private long recentMaxFlushTime;

        private long ticksTakenForLastFlush;

        private long totalTicksTakenForFlushes;

        #endregion Fields

        #region Constructors and Finalizers

        public MetricsEnabledConcurrentStore(IConcurrentStore<T> store)
        {
            this.store = store;
            this.store.OnStoreFlush += this._store_OnStoreFlush;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<StoreFlushEventArgs<T>> OnStoreFlush;

        #endregion Public Events

        #region Public Properties

        [PerfCounter(GroupName = "Flush KPIs")]
        public long AverageNumberOfItemsPerFlush
        {
            get
            {
                return this.numberOfFlushes == 0 ? Interlocked.Read(ref this.numberOfItemsWritten) : this.numberOfItemsFlushed / this.numberOfFlushes;
            }
        }

        [PerfCounter(GroupName = "Flush KPIs", CounterTimeTypes = TimeTypes.Milliseconds)]
        public TimeSpan AverageTimeTakenForFlushes
        {
            get
            {
                return TimeSpan.FromTicks(this.numberOfFlushes == 0 ? 0 : (this.totalTicksTakenForFlushes / this.numberOfFlushes));
            }
        }

        public IConcurrentStore<T> InnerStore
        {
            get
            {
                return this.store;
            }
        }

        [PerfCounter(GroupName = "Flush KPIs", CounterTimeTypes = TimeTypes.Milliseconds)]
        public TimeSpan MaxTimeTakenForFlush
        {
            get
            {
                return TimeSpan.FromTicks(this.maxTicksTakenForFlush);
            }
        }

        public long MaxWaitTime
        {
            get
            {
                return this.store.MaxWaitTime;
            }
        }

        [PerfCounter(GroupName = "Flush KPIs")]
        public long NumberOfFlushes
        {
            get
            {
                return this.numberOfFlushes;
            }
        }

        [PerfCounter(GroupName = "Write KPIs")]
        public long NumberOfItemsAttemptedToWrite
        {
            get
            {
                return Interlocked.Read(ref this.numberOfItemsAttemptedToWrite);
            }
        }

        [PerfCounter(GroupName = "Flush KPIs")]
        public long NumberOfItemsFlushed
        {
            get
            {
                return this.numberOfItemsFlushed;
            }
        }

        [PerfCounter(GroupName = "Write KPIs")]
        public long NumberOfItemsSkipped
        {
            get
            {
                return Interlocked.Read(ref this.numberOfItemsSkipped);
            }
        }

        [PerfCounter(GroupName = "Write KPIs")]
        public long NumberOfItemsWritten
        {
            get
            {
                return Interlocked.Read(ref this.numberOfItemsWritten);
            }
        }

        [PerfCounter(GroupName = "Flush KPIs", CounterTimeTypes = TimeTypes.Milliseconds)]
        public TimeSpan RecentMaxFlushTime
        {
            get
            {
                long value = this.recentMaxFlushTime;
                this.recentMaxFlushTime = this.ticksTakenForLastFlush;
                return TimeSpan.FromTicks(value);
            }
        }

        [PerfCounter(GroupName = "Flush KPIs", CounterTimeTypes = TimeTypes.Milliseconds)]
        public TimeSpan TimeTakenForLastFlush
        {
            get
            {
                return TimeSpan.FromTicks(this.ticksTakenForLastFlush);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool Add(T item)
        {
            Interlocked.Increment(ref this.numberOfItemsAttemptedToWrite);
            bool isWritten = this.store.Add(item);
            if (isWritten)
            {
                Interlocked.Increment(ref this.numberOfItemsWritten);
            }
            else
            {
                Interlocked.Increment(ref this.numberOfItemsSkipped);
            }

            return isWritten;
        }

        public void Dispose()
        {
            this.store.Dispose();
        }

        public void ForceFlush()
        {
            this.store.ForceFlush();
        }

        #endregion Public Methods and Operators

        #region Methods

        private void _store_OnStoreFlush(object sender, StoreFlushEventArgs<T> e)
        {
            if (this.OnStoreFlush != null)
            {
                long ticksBefore = SharedStopWatch.ElapsedTicks;
                try
                {
                    this.OnStoreFlush(this, e);
                }
                finally
                {
                    long ticksTaken = SharedStopWatch.ElapsedTicks - ticksBefore;
                    this.totalTicksTakenForFlushes += ticksTaken;
                    this.ticksTakenForLastFlush = ticksTaken;
                    if (ticksTaken > this.maxTicksTakenForFlush)
                    {
                        this.maxTicksTakenForFlush = ticksTaken;
                    }

                    if (ticksTaken > this.recentMaxFlushTime)
                    {
                        this.recentMaxFlushTime = ticksTaken;
                    }
                }
            }

            this.numberOfItemsFlushed += e.ToIndex - e.FromIndex + 1;
            this.numberOfFlushes++;
        }

        #endregion Methods
    }
}
