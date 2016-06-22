using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class SafeHashSet<T> : AbstractReaderWriterAtomicOperationSupported, ICollection<T>
    {
        #region Fields

        private readonly HashSet<T> adaptedObject = new HashSet<T>();

        #endregion Fields

        #region Public Properties

        public int Count
        {
            get
            {
                return this.adaptedObject.Count;
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

        public bool Add(T item)
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                return this.adaptedObject.Add(item);
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                this.adaptedObject.Clear();
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            this.LockSlim.EnterReadLock();
            try
            {
                return this.adaptedObject.Contains(item);
            }
            finally
            {
                this.LockSlim.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                this.LockSlim.EnterReadLock();
                this.adaptedObject.CopyTo(array, arrayIndex);
            }
            finally
            {
                this.LockSlim.ExitReadLock();
            }
        }

        public List<T> GetAll()
        {
            this.LockSlim.EnterReadLock();
            try
            {
                return this.adaptedObject.ToList();
            }
            finally
            {
                this.LockSlim.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeEnumeratorDecorator<T>(this.adaptedObject.GetEnumerator(), this.LockSlim);
        }

        public bool Remove(T item)
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                return this.adaptedObject.Remove(item);
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion Explicit Interface Methods

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.LockSlim.Dispose();
        }

        #endregion Methods
    }
}
