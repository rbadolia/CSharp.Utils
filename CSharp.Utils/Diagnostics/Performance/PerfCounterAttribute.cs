using System;
using System.Diagnostics;

namespace CSharp.Utils.Diagnostics.Performance
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PerfCounterAttribute : Attribute
    {
        #region Constructors and Finalizers

        public PerfCounterAttribute()
        {
            this.CounterType = PerformanceCounterType.NumberOfItems64;
            this.SizeType = SizeTypes.None;
            this.CounterSizeTypes = SizeTypes.None;
            this.CounterTimeTypes = TimeTypes.None;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public SizeTypes CounterSizeTypes { get; set; }

        public TimeTypes CounterTimeTypes { get; set; }

        public PerformanceCounterType CounterType { get; private set; }

        public string GroupName { get; set; }

        public SizeTypes SizeType { get; set; }

        #endregion Public Properties
    }
}
