using System;
using System.Threading;

namespace CSharp.Utils.Diagnostics.Performance
{
    [PerfCounterCategory("DotNet Performance")]
    public sealed class DotNetPerfCounters
    {
        #region Static Fields

        private static readonly DotNetPerfCounters InstanceObject = new DotNetPerfCounters();

        #endregion Static Fields

        #region Constructors and Finalizers

        private DotNetPerfCounters()
        {
            this.IsStarted = false;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static DotNetPerfCounters Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int AvailableCompletionPortThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetAvailableThreads(out i, out j);
                return j;
            }
        }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int AvailableWorkerThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetAvailableThreads(out i, out j);
                return i;
            }
        }

        [PerfCounter(GroupName = ".NET Memory")]
        public int Gen0CollectionCount
        {
            get
            {
                return GC.CollectionCount(0);
            }
        }

        [PerfCounter(GroupName = ".NET Memory")]
        public int Gen1CollectionCount
        {
            get
            {
                return GC.CollectionCount(1);
            }
        }

        [PerfCounter(GroupName = ".NET Memory")]
        public int Gen2CollectionCount
        {
            get
            {
                return GC.CollectionCount(2);
            }
        }

        public bool IsStarted { get; private set; }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int MaximumCompletionPortThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetMaxThreads(out i, out j);
                return j;
            }
        }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int MaximumWorkerThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetMaxThreads(out i, out j);
                return i;
            }
        }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int MinimumCompletionPortThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetMinThreads(out i, out j);
                return j;
            }
        }

        [PerfCounter(GroupName = "ThreadPool Performance")]
        public int MinimumWorkerThreads
        {
            get
            {
                int i;
                int j;
                ThreadPool.GetMinThreads(out i, out j);
                return i;
            }
        }

        [PerfCounter(GroupName = ".NET Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long TotalMemory
        {
            get
            {
                return GC.GetTotalMemory(false);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            this.Stop();
        }

        public void Start()
        {
            if (!this.IsStarted)
            {
                this.IsStarted = true;
                PerfCounterManager.Instance.AddInstance(this, "Unique Instance", CounterStorageType.Memory);
            }
        }

        public void Stop()
        {
            if (this.IsStarted)
            {
                this.IsStarted = false;
                PerfCounterManager.Instance.RemoveInstance(this);
            }
        }

        #endregion Public Methods and Operators
    }
}
