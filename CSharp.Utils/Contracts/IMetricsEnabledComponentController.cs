using System;
using CSharp.Utils.Diagnostics.Performance;

namespace CSharp.Utils.Contracts
{
    [PerfCounterCategory("Metrics Enabled Component Controllers")]
    public interface IMetricsEnabledComponentController : IComponentController
    {
        #region Public Properties

        [PerfCounter]
        double BusyPercentage { get; }

        [PerfCounter]
        double IdlePercentage { get; }

        [PerfCounter]
        long NumberOfThreadsBeingServed { get; }

        [PerfCounter]
        TimeSpan TimeSpentInBusy { get; }

        [PerfCounter]
        TimeSpan TimeSpentInIdle { get; }

        [PerfCounter]
        TimeSpan TimeSpentInPaused { get; }

        [PerfCounter]
        TimeSpan TimeSpentInStopped { get; }

        #endregion Public Properties
    }
}
