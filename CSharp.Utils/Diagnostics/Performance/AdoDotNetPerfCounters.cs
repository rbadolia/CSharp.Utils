using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace CSharp.Utils.Diagnostics.Performance
{
    [PerfCounterCategory("AdoDotNet Performance")]
    public sealed class AdoDotNetPerfCounters : AbstractDisposable
    {
        #region Fields

        private readonly string[] counterNames = { "NumberOfActiveConnectionPools", "NumberOfInactiveConnectionPools", "NumberOfActiveConnectionPoolGroups", "NumberOfInactiveConnectionPoolGroups", "HardConnectsPerSecond", "HardDisconnectsPerSecond", "NumberOfPooledConnections", "NumberOfNonPooledConnections", "NumberOfStasisConnections", "NumberOfReclaimedConnections", 
                                                    "SoftConnectsPerSecond", "SoftDisconnectsPerSecond", "NumberOfActiveConnections", "NumberOfFreeConnections" };

        private BitArray bitArray;

        private PerformanceCounter[] perfCounters;

        #endregion Fields

        #region Constructors and Finalizers

        public AdoDotNetPerfCounters()
        {
            this.IsStarted = false;
            this.EnableExpensiveCounters = true;
            this.CategoryName = ".NET Data Provider for SqlServer";
            this.Name = "SqlServer";
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryName { get; set; }

        public bool EnableExpensiveCounters { get; set; }

        [PerfCounter]
        public float HardConnectsPerSecond
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[4])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[4].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float HardDisconnectsPerSecond
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[5])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[5].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        public bool IsStarted { get; private set; }

        public string Name { get; set; }

        [PerfCounter]
        public float NumberOfActiveConnectionPoolGroups
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[2])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[2].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfActiveConnectionPools
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[0])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[0].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfActiveConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[12])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[12].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfFreeConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[12])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[13].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfInactiveConnectionPoolGroups
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[3])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[3].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfInactiveConnectionPools
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[1])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[1].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfNonPooledConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[7])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[7].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfPooledConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[6])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[6].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfReclaimedConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[9])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[9].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float NumberOfStasisConnections
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[8])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[8].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float SoftConnectsPerSecond
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[10])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[10].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        [PerfCounter]
        public float SoftDisconnectsPerSecond
        {
            get
            {
                if (this.bitArray == null || !this.bitArray[11])
                {
                    return -1;
                }

                try
                {
                    return this.perfCounters[11].NextValue();
                }
                catch
                {
                    return -1;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Start()
        {
            if (!this.IsStarted)
            {
                string instanceName = this.getInstanceName();
                if (instanceName != null)
                {
                    this.setUpPerformanceCounters(instanceName);
                    PerfCounterManager.Instance.AddInstance(this, this.Name, CounterStorageType.Memory);
                }
            }

            this.IsStarted = true;
        }

        public void Stop()
        {
            if (this.IsStarted)
            {
                this.IsStarted = false;
                PerfCounterManager.Instance.RemoveInstance(this);

                foreach (PerformanceCounter counter in this.perfCounters)
                {
                    try
                    {
                        counter.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.Stop();
        }

        private string getInstanceName()
        {
            string endString = "[" + ProcessHelper.CurrentProcess.Id.ToString(CultureInfo.InvariantCulture) + "]";
            var category = new PerformanceCounterCategory(this.CategoryName);
            foreach (string instanceName in category.GetInstanceNames())
            {
                if (instanceName.EndsWith(endString))
                {
                    return instanceName;
                }
            }

            return null;
        }

        private void setUpPerformanceCounters(string instanceName)
        {
            this.bitArray = new BitArray(this.counterNames.Length);
            this.bitArray.SetAll(false);
            int limit = this.EnableExpensiveCounters ? this.counterNames.Length : this.counterNames.Length - 4;
            this.perfCounters = new PerformanceCounter[limit];
            for (int i = 0; i < limit; i++)
            {
                try
                {
                    this.perfCounters[i] = new PerformanceCounter { CategoryName = this.CategoryName, CounterName = this.counterNames[i], InstanceName = instanceName };
                    this.bitArray[i] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        #endregion Methods
    }
}
