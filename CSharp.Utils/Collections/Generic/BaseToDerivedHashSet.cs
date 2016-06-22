using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class BaseToDerivedHashSet<TBase, TDerived> : HashSet<TBase>, ICollection<TDerived>
        where TDerived : class, TBase
    {
        void ICollection<TDerived>.Add(TDerived item)
        {
            this.Add(item);
        }

        void ICollection<TDerived>.Clear()
        {
            this.Clear();
        }

        bool ICollection<TDerived>.Contains(TDerived item)
        {
            return this.Contains(item);
        }

        void ICollection<TDerived>.CopyTo(TDerived[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        int ICollection<TDerived>.Count
        {
            get { return this.Count; }
        }

        bool ICollection<TDerived>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<TDerived>.Remove(TDerived item)
        {
            return this.Remove(item);
        }

        IEnumerator<TDerived> IEnumerable<TDerived>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return (TDerived) item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return (TDerived)item;
            }
        }
    }
}
