using System.Collections.Generic;
using System.Runtime.Serialization;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{
    public class SafeListDecorator<T> : SafeCollectionDecorator<T>, IList<T>, ISerializable
    {
        #region Fields

        private readonly IList<T> adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        public SafeListDecorator(IList<T> adaptedObject)
            : base(adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Indexers

        public T this[int index]
        {
            get
            {
                try
                {
                    this.LockSlim.EnterReadLock();
                    return this.adaptedObject[index];
                }
                finally
                {
                    this.LockSlim.ExitReadLock();
                }
            }

            set
            {
                try
                {
                    this.LockSlim.EnterWriteLock();
                    this.adaptedObject[index] = value;
                }
                finally
                {
                    this.LockSlim.ExitWriteLock();
                }
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("adaptedObject", this.adaptedObject, this.adaptedObject.GetType());
        }

        public int IndexOf(T item)
        {
            try
            {
                this.LockSlim.EnterReadLock();
                return this.adaptedObject.IndexOf(item);
            }
            finally
            {
                this.LockSlim.ExitReadLock();
            }
        }

        public void Insert(int index, T item)
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                this.adaptedObject.Insert(index, item);
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            try
            {
                this.LockSlim.EnterWriteLock();
                this.adaptedObject.RemoveAt(index);
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        #endregion Public Methods and Operators
    }
}
