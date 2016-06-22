using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{
    public class SafeCollectionDecorator<T> : AbstractReaderWriterAtomicOperationSupported, ICollection<T>
    {
        #region Fields

        private readonly ICollection<T> adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        public SafeCollectionDecorator(ICollection<T> adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Count
        {
            get
            {
                try
                {
                    this.LockSlim.EnterReadLock();
                    return this.adaptedObject.Count;
                }
                finally
                {
                    this.LockSlim.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                try
                {
                    this.LockSlim.EnterReadLock();
                    return this.adaptedObject.IsReadOnly;
                }
                finally
                {
                    this.LockSlim.ExitReadLock();
                }
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Add(T item)
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                this.adaptedObject.Add(item);
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
            try
            {
                this.LockSlim.EnterReadLock();
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

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetEnumeratorCore();
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorCore();
        }

        #endregion Explicit Interface Methods

        #region Methods

        private IEnumerator<T> GetEnumeratorCore()
        {
            IEnumerator<T> enumerator = this.adaptedObject.GetEnumerator();
            enumerator = new SafeEnumeratorDecorator<T>(enumerator, this.LockSlim);
            return enumerator;
        }

        #endregion Methods
    }
}
