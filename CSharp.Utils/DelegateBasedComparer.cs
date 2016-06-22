using System;
using System.Collections.Generic;

namespace CSharp.Utils
{
    public class DelegateBasedComparer<T> : IComparer<T>
    {
        #region Fields

        private readonly Comparison<T> comparison;

        #endregion Fields
        #region Constructors and Finalizers

        public DelegateBasedComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public int Compare(T x, T y)
        {
            return this.comparison(x, y);
        }

        #endregion Public Methods and Operators
    }
}
