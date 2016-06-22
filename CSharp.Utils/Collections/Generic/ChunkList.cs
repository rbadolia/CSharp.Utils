using System;
using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Algorithms;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class ChunkList<T> : ChunkArray<T>, IList<T>
    {
        #region Fields

        protected int _itemsCount = 0;

        #endregion Fields

        #region Constructors and Finalizers

        public ChunkList()
            : this(256)
        {
        }

        public ChunkList(int chunkSize)
            : base(chunkSize, chunkSize)
        {
        }

        public ChunkList(IEnumerable<T> elements)
            : this()
        {
            this.Add(elements);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Count
        {
            get
            {
                return this._itemsCount;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static ChunkList<T> Create(IEnumerable<T> items)
        {
            var a = new ChunkList<T> { items };
            return a;
        }

        public void Add(T element)
        {
            this.ResizeIfRequired();
            base[this._itemsCount] = element;
            this._itemsCount++;
        }

        public void Add(IEnumerable<T> elements)
        {
            foreach (T element in elements)
            {
                this.Add(element);
            }
        }

        public override void Clear()
        {
            this.capacity = this.chunkSize;
            this.InitializeProtected();
            this._itemsCount = 0;
        }

        public bool Contains(T item)
        {
            int index = this.IndexOf(item);
            return index > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < this._itemsCount; i++)
            {
                array[i + arrayIndex] = base[i];
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return new DataStructureEnumerator<T>(this, 0, this._itemsCount);
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < this._itemsCount; i++)
            {
                if (EqualityComparer<T>.Default.Equals(base[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index >= this._itemsCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            this.ResizeIfRequired();
            this._itemsCount++;
            for (int i = this._itemsCount - 1; i >= index; i--)
            {
                base[i + 1] = base[i];
            }

            base[index] = item;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index > -1)
            {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this._itemsCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            base[index] = this.DefaultValue;
            for (int i = index + 1; i < this._itemsCount; i++)
            {
                base[i - 1] = base[i];
            }

            this._itemsCount--;
            if (this._itemsCount >= this.chunkSize && this._itemsCount % this.chunkSize == 0)
            {
                this.ChunkArrays.RemoveAt(this.ChunkArrays.Count - 1);
            }
            else
            {
                base[this._itemsCount] = this.DefaultValue;
            }
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataStructureEnumerator<T>(this, 0, this._itemsCount);
        }

        #endregion Explicit Interface Methods

        public void Sort()
        {
            this.MergeSort();
        }

        public void Sort(IComparer<T> comparer)
        {
            this.MergeSort(comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            this.MergeSort(comparison);
        }

        #region Methods

        private void ResizeIfRequired()
        {
            if (this._itemsCount >= this.capacity)
            {
                this.ChunkArrays.Add(new T[this.chunkSize]);
                this.capacity += this.chunkSize;
            }
        }

        #endregion Methods
    }
}
