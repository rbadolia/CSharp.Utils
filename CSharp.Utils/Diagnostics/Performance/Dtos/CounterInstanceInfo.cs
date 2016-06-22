using System;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    [Serializable]
    public class CounterInstanceInfo
    {
        #region Public Properties

        public double[] CounterValues { get; set; }

        public string InstanceName { get; set; }

        public object[] ReferenceTypeCounterValues { get; set; }

        #endregion Public Properties
    }
}
