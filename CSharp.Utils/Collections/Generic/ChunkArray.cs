using System;
using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Diagnostics;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class ChunkArray<T> : IEnumerable<T>, IDataStructureAdapter<T>
    {
        #region Static Fields

        private static readonly int OptimalChunkSize;

        #endregion Static Fields

        #region Fields

        protected readonly int chunkSize;

        protected int capacity;

        protected List<T[]> ChunkArrays;

        protected T DefaultValue = default(T);

        #endregion Fields

        #region Constructors and Finalizers

        static ChunkArray()
        {
            OptimalChunkSize = ProcessHelper.LargeObjectThreshold;
        }

        public ChunkArray(int capacity)
        {
            this.capacity = capacity;
            this.chunkSize = Math.Min(capacity, OptimalChunkSize);
            this.InitializeProtected();
        }

        public ChunkArray(int capacity, int chunkSize)
        {
            this.capacity = capacity;
            this.chunkSize = Math.Min(Math.Min(chunkSize, capacity), OptimalChunkSize);
            this.InitializeProtected();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Capacity
        {
            get
            {
                return this.capacity;
            }
        }

        public int ChunkSize
        {
            get
            {
                return this.chunkSize;
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public virtual T this[int index]
        {
            get
            {
                int x = index / this.chunkSize;
                int y = index % this.chunkSize;
                return this.ChunkArrays[x][y];
            }

            set
            {
                int x = index / this.chunkSize;
                int y = index % this.chunkSize;
                this.ChunkArrays[x][y] = value;
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public static ChunkArray<T> CreateFromList(IList<T> items)
        {
            var ba = new ChunkArray<T>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                ba[i] = items[i];
            }

            return ba;
        }

        public static T[] ToArray(ChunkArray<T> chunkArray)
        {
            var array = new T[chunkArray.capacity];
            for (int i = 0; i < chunkArray.capacity; i++)
            {
                array[i] = chunkArray[i];
            }

            return array;
        }

        public static TTarget[] ToCastedArray<TTarget>(ChunkArray<T> chunkArray) where TTarget : class
        {
            var array = new TTarget[chunkArray.capacity];
            for (int i = 0; i < chunkArray.capacity; i++)
            {
                array[i] = chunkArray[i] as TTarget;
            }

            return array;
        }

        public virtual void Clear()
        {
            foreach (var chunkArray in this.ChunkArrays)
            {
                for (int j = 0; j < chunkArray.Length; j++)
                {
                    chunkArray[j] = this.DefaultValue;
                }
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new DataStructureEnumerator<T>(this, 0, this.capacity);
        }

        public T[] ToArray()
        {
            return ToArray(this);
        }

        public TTarget[] ToCastedArray<TTarget>() where TTarget : class
        {
            return ToCastedArray<TTarget>(this);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataStructureEnumerator<T>(this, 0, this.capacity);
        }

        #endregion Explicit Interface Methods

        #region Methods

        protected void InitializeProtected()
        {
            int reminder = this.capacity % this.chunkSize;
            int numberOfChunks = this.capacity / this.chunkSize;
            if (reminder > 0)
            {
                numberOfChunks++;
            }

            this.ChunkArrays = new List<T[]>(numberOfChunks);
            for (int i = 0; i < numberOfChunks - 1; i++)
            {
                this.ChunkArrays.Add(new T[this.chunkSize]);
            }

            this.ChunkArrays.Add(new T[reminder > 0 ? reminder : this.chunkSize]);
        }

        #endregion Methods
    }
}
