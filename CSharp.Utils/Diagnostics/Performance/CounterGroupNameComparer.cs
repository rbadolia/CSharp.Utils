using System.Collections.Generic;
using System.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    internal sealed class CounterGroupNameComparer : IComparer<KeyValuePair<PropertyInfo, PerfCounterAttribute>>
    {
        #region Static Fields

        private static readonly CounterGroupNameComparer InstanceObject = new CounterGroupNameComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private CounterGroupNameComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static CounterGroupNameComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(KeyValuePair<PropertyInfo, PerfCounterAttribute> x, KeyValuePair<PropertyInfo, PerfCounterAttribute> y)
        {
            int result = string.CompareOrdinal(x.Value.GroupName, y.Value.GroupName);
            return result == 0 ? string.CompareOrdinal(x.Key.Name, y.Key.Name) : result;
        }

        #endregion Public Methods and Operators
    }
}
