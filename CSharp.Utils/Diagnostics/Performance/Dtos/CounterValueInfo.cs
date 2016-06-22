using System;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    [Serializable]
    public class CounterValueInfo
    {
        #region Constructors and Finalizers

        public CounterValueInfo()
        {
        }

        public CounterValueInfo(string counterName, double rawValue)
        {
            this.CounterName = counterName;
            this.RawValue = rawValue;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CounterName { get; set; }

        public double RawValue { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static CounterValueInfo CreateNew(string counterName, double rawValue)
        {
            return new CounterValueInfo(counterName, rawValue);
        }

        public override string ToString()
        {
            return DynamicToStringHelper<CounterValueInfo>.ExportAsString(this);
        }

        #endregion Public Methods and Operators
    }
}
