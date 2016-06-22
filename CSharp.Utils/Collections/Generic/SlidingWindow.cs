using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class SlidingWindow<T>
    {
        #region Fields

        private readonly T[] _array;

        private bool _isCycleCompleted;

        private long _lastWrittenIndex = -1;

        #endregion Fields

        #region Constructors and Finalizers

        public SlidingWindow(long size)
        {
            this._array = new T[size];
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public long ItemsCount
        {
            get
            {
                return this._isCycleCompleted ? this._array.LongLength : this._lastWrittenIndex;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Add(T item)
        {
            long index = (this._lastWrittenIndex + 1) % this._array.LongLength;
            this._array[index] = item;
            if (index < this._lastWrittenIndex)
            {
                this._isCycleCompleted = true;
            }

            this._lastWrittenIndex = index;
        }

        public IEnumerable<T> GetAllItems()
        {
            long startIndex = this._isCycleCompleted ? (this._lastWrittenIndex + 1) : 0;
            long endIndex = this._isCycleCompleted ? (this._lastWrittenIndex + this._array.LongLength) : this._lastWrittenIndex;
            long index = startIndex;
            for (long i = index; i <= endIndex; i++)
            {
                yield return this._array[i % this._array.LongLength];
            }
        }

        public bool TryGetLast(out T item)
        {
            item = default(T);
            if (this._lastWrittenIndex > -1)
            {
                item = this._array[this._lastWrittenIndex];
                return true;
            }

            return false;
        }

        #endregion Public Methods and Operators
    }
}
