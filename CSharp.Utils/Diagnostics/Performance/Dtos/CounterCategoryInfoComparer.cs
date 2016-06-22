using System;
using System.Collections.Generic;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    public sealed class CounterCategoryInfoComparer : IComparer<CounterCategoryInfo>
    {
        #region Static Fields

        private static readonly CounterCategoryInfoComparer InstanceObject = new CounterCategoryInfoComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private CounterCategoryInfoComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static CounterCategoryInfoComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(CounterCategoryInfo x, CounterCategoryInfo y)
        {
            return string.Compare(x.CategoryName, y.CategoryName, StringComparison.Ordinal);
        }

        #endregion Public Methods and Operators
    }
}
