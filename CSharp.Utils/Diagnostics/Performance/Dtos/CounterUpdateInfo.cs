using System;
using System.Collections.Generic;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    [Serializable]
    public class CounterUpdateInfo
    {
        #region Constructors and Finalizers

        public CounterUpdateInfo()
        {
            this.ProcessInfo = new ProcessInfo();
            this.Categories = new List<CounterCategoryInfo>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public List<CounterCategoryInfo> Categories { get; set; }

        public ProcessInfo ProcessInfo { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public CounterCategoryInfo GetCategoryInfo(string categoryName, bool ignoreCase)
        {
            foreach (CounterCategoryInfo categoryInfo in this.Categories)
            {
                if (string.Compare(categoryInfo.CategoryName, categoryName, ignoreCase) == 0)
                {
                    return categoryInfo;
                }
            }

            return null;
        }

        public double? GetCounterValue(string categoryName, string instanceName, string counterName, bool ignoreCase)
        {
            CounterCategoryInfo category = this.GetCategoryInfo(categoryName, ignoreCase);
            if (category != null)
            {
                return category.GetCounterValue(instanceName, counterName, ignoreCase);
            }

            return null;
        }

        #endregion Public Methods and Operators
    }
}
