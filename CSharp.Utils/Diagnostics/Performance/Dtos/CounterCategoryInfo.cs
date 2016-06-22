using System;
using System.Collections.Generic;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    [Serializable]
    public class CounterCategoryInfo
    {
        #region Constructors and Finalizers

        public CounterCategoryInfo()
        {
            this.Instances = new List<CounterInstanceInfo>();
            this.CounterNames = new List<string>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryName { get; set; }

        public List<string> CounterNames { get; set; }

        public List<CounterInstanceInfo> Instances { get; set; }

        public List<string> ReferenceTypeCounterNames { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public double? GetCounterValue(string instanceName, string counterName, bool ignoreCase)
        {
            int index = -1;
            for (int i = 0; i < this.CounterNames.Count; i++)
            {
                if (string.Compare(this.CounterNames[i], counterName, ignoreCase) == 0)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                CounterInstanceInfo instance = this.GetInstanceInfo(instanceName, ignoreCase);
                if (instance != null)
                {
                    return instance.CounterValues[index];
                }
            }

            return null;
        }

        public CounterInstanceInfo GetInstanceInfo(string instanceName, bool ignoreCase)
        {
            foreach (CounterInstanceInfo instance in this.Instances)
            {
                if (string.Compare(instance.InstanceName, instanceName, ignoreCase) == 0)
                {
                    return instance;
                }
            }

            return null;
        }

        #endregion Public Methods and Operators
    }
}
