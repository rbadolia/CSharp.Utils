using System.Collections.Generic;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class SafeLinkedList<T> : SafeCollectionDecorator<T>
    {
        #region Fields

        private readonly LinkedList<T> adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        public SafeLinkedList(LinkedList<T> adaptedObject)
            : base(adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public T RemoveFirst()
        {
            this.LockSlim.EnterUpgradeableReadLock();
            try
            {
                LinkedListNode<T> firstNode = this.adaptedObject.First;
                if (firstNode == null)
                {
                    this.adaptedObject.RemoveFirst();
                    return default(T);
                }

                this.LockSlim.EnterWriteLock();
                try
                {
                    this.adaptedObject.RemoveFirst();
                    return firstNode.Value;
                }
                finally
                {
                    this.LockSlim.ExitWriteLock();
                }
            }
            finally
            {
                this.LockSlim.ExitUpgradeableReadLock();
            }
        }

        public bool TryRemoveFirst(out T item)
        {
            item = default(T);
            this.LockSlim.EnterUpgradeableReadLock();
            try
            {
                LinkedListNode<T> firstNode = this.adaptedObject.First;
                if (firstNode == null)
                {
                    return false;
                }

                this.LockSlim.EnterWriteLock();
                try
                {
                    this.adaptedObject.RemoveFirst();
                    item = firstNode.Value;
                    return true;
                }
                finally
                {
                    this.LockSlim.ExitWriteLock();
                }
            }
            finally
            {
                this.LockSlim.ExitUpgradeableReadLock();
            }
        }

        #endregion Public Methods and Operators
    }
}
