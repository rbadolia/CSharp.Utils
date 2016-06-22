using System;
using System.Diagnostics;

namespace CSharp.Utils.Diagnostics.Performance
{
    [PerfCounterCategory("Process Performance")]
    public sealed class ProcessPerfCounters : AbstractDisposable, IRefreshCounters
    {
        #region Static Fields

        private static readonly ProcessPerfCounters InstanceObject = new ProcessPerfCounters();

        #endregion Static Fields

        #region Fields

        private double _previousProcessCpuUsage;

        private float previousSystemCpuUsage;

        private int previousThreadCount;

        private PerformanceCounter processCpuCounter;

        private PerformanceCounter systemCpuCounter;

        #endregion Fields
        #region Constructors and Finalizers

        private ProcessPerfCounters()
        {
            this.IsStarted = false;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static ProcessPerfCounters Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        [PerfCounter(GroupName = "Process Performance")]
        public int BasePriority
        {
            get
            {
                return ProcessHelper.CurrentProcess.BasePriority;
            }
        }

        [PerfCounter(GroupName = "Process Performance")]
        public int HandleCount
        {
            get
            {
                return ProcessHelper.CurrentProcess.HandleCount;
            }
        }

        public bool IsStarted { get; private set; }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PagedMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.PagedMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PagedSystemMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.PagedSystemMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PeakPagedMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.PeakPagedMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PeakVirtualMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.PeakVirtualMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PeakWorkingSet
        {
            get
            {
                return ProcessHelper.CurrentProcess.PeakWorkingSet64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long PrivateMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.PrivateMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Performance", CounterTimeTypes = TimeTypes.Minutes)]
        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                return ProcessHelper.CurrentProcess.PrivilegedProcessorTime;
            }
        }

        [PerfCounter(GroupName = "Process Performance")]
        public double ProcessCpuUsage
        {
            get
            {
                try
                {
                    if (this.processCpuCounter != null)
                    {
                        this._previousProcessCpuUsage = this.processCpuCounter.NextValue() / Environment.ProcessorCount;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return this._previousProcessCpuUsage;
            }
        }

        [PerfCounter(GroupName = "System Performance")]
        public float SystemCpuUsage
        {
            get
            {
                try
                {
                    if (this.systemCpuCounter != null)
                    {
                        this.previousSystemCpuUsage = this.systemCpuCounter.NextValue();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return this.previousSystemCpuUsage;
            }
        }

        [PerfCounter(GroupName = "Process Performance")]
        public long ThreadCount
        {
            get
            {
                ProcessThreadCollection coll = ProcessHelper.CurrentProcess.Threads;
                this.previousThreadCount = coll.Count;
                return this.previousThreadCount;
            }
        }

        [PerfCounter(GroupName = "Process Performance", CounterTimeTypes = TimeTypes.Minutes)]
        public TimeSpan TotalProcessorTime
        {
            get
            {
                return ProcessHelper.CurrentProcess.TotalProcessorTime;
            }
        }

        [PerfCounter(GroupName = "Process Performance", CounterTimeTypes = TimeTypes.Minutes)]
        public TimeSpan UserProcessorTime
        {
            get
            {
                return ProcessHelper.CurrentProcess.UserProcessorTime;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long VirtualMemorySize
        {
            get
            {
                return ProcessHelper.CurrentProcess.VirtualMemorySize64;
            }
        }

        [PerfCounter(GroupName = "Process Memory", SizeType = SizeTypes.Bytes, CounterSizeTypes = SizeTypes.MegaBytes)]
        public long WorkingSet
        {
            get
            {
                return ProcessHelper.CurrentProcess.WorkingSet64;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void RefreshCounters()
        {
            try
            {
                ProcessHelper.CurrentProcess.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Start()
        {
            if (!this.IsStarted)
            {
                if (!GlobalSettings.Instance.IsService)
                {
                    try
                    {
                        this.systemCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                    string currentProcessInstanceName = ProcessHelper.CurrentProcessInstanceName;
                    if (currentProcessInstanceName != null)
                    {
                        this.processCpuCounter = new PerformanceCounter("Process", "% Processor Time", currentProcessInstanceName, true);
                    }
                }

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
                if (this.systemCpuCounter != null)
                {
                    this.systemCpuCounter.Dispose();
                }

                if (this.processCpuCounter != null)
                {
                    this.processCpuCounter.Dispose();
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.Stop();
        }

        #endregion Methods
    }
}
