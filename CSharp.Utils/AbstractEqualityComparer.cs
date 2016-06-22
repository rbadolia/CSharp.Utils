using System;
using System.Collections.Generic;

namespace CSharp.Utils
{
    [Serializable]
    public abstract class AbstractEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return this.EqualsProtected(x, y);
        }

        protected abstract bool EqualsProtected(T x, T y);

        public virtual int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
