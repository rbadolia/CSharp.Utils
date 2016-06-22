using System.Collections.Generic;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class CustomPerfCounters : AbstractDisposable
    {
        #region Public Properties

        public string CategoryName { get; set; }

        public List<CustomCounterInfo> Counters { get; set; }

        public bool IsStarted { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Start()
        {
            if (!this.IsStarted)
            {
                this.IsStarted = true;
                PerfCounterManager.Instance.AddCustomPerfCounters(this);
            }
        }

        public void Stop()
        {
            if (this.IsStarted)
            {
                this.IsStarted = false;
                PerfCounterManager.Instance.RemoveCustomPerfCounters(this.CategoryName);
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
