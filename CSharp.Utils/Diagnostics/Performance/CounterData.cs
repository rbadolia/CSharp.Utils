using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class CounterData
    {
        #region Constructors and Finalizers

        public CounterData()
        {
        }

        public CounterData(DateTime timeStamp, double rawValue)
        {
            this.TimeStamp = timeStamp;
            this.RawValue = rawValue;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public double RawValue { get; set; }

        public DateTime TimeStamp { get; set; }

        #endregion Public Properties
    }
}
