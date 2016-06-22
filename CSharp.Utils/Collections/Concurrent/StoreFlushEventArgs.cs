using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class StoreFlushEventArgs<T> : EventArgs, IEnumerable<T>
    {
        #region Constructors and Finalizers

        public StoreFlushEventArgs(T[] storeData)
            : this(storeData, 0, storeData.Length - 1)
        {
        }

        public StoreFlushEventArgs(T[] storeData, int fromIndex, int toIndex)
        {
            this.StoreData = storeData;
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Length
        {
            get
            {
                return this.ToIndex - this.FromIndex + 1;
            }
        }

        #endregion Public Properties

        #region Properties

        internal int FromIndex { get; private set; }

        internal T[] StoreData { get; private set; }

        internal int ToIndex { get; private set; }

        #endregion Properties

        #region Public Indexers

        public T this[int index]
        {
            get
            {
                return this.StoreData[index + this.FromIndex];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = this.FromIndex; i <= this.ToIndex; i++)
            {
                yield return this.StoreData[i];
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(this.StoreData, this.FromIndex, this.ToIndex - this.FromIndex + 1);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = this.FromIndex; i <= this.ToIndex; i++)
            {
                yield return this.StoreData[i];
            }
        }

        #endregion Explicit Interface Methods
    }
}
