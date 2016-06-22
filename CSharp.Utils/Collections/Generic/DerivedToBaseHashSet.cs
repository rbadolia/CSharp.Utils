using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class DerivedToBaseHashSet<TDerived, TBase> : HashSet<TDerived>, ICollection<TBase>
        where TDerived : class, TBase
    {
        void ICollection<TBase>.Add(TBase item)
        {
            var derived = (TDerived)item;
            this.Add(derived);
        }

        void ICollection<TBase>.Clear()
        {
            this.Clear();
        }

        bool ICollection<TBase>.Contains(TBase item)
        {
            var derived = (TDerived)item;
            return this.Contains(derived);
        }

        void ICollection<TBase>.CopyTo(TBase[] array, int arrayIndex)
        {
            int index = 0;
            foreach (var v in this)
            {
                array[index + arrayIndex] = v;
                index++;
            }
        }

        int ICollection<TBase>.Count
        {
            get { return this.Count; }
        }

        bool ICollection<TBase>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<TBase>.Remove(TBase item)
        {
            var derived = (TDerived)item;
            return this.Remove(derived);
        }

        IEnumerator<TBase> IEnumerable<TBase>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item;
            }
        }
    }
}
