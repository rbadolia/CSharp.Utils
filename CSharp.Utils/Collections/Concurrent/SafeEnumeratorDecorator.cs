using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class SafeEnumeratorDecorator<T> : AbstractDisposable, IEnumerator<T>
    {
        #region Fields

        private readonly IEnumerator<T> adaptedObject;

        private readonly ReaderWriterLock readerWriterLock;

        private readonly ReaderWriterLockSlim readerWriterLockSlim;

        private volatile bool hasLockAcquired;

        #endregion Fields

        #region Constructors and Finalizers

        public SafeEnumeratorDecorator(IEnumerator<T> adaptedObject, ReaderWriterLock readerWriterLock)
            : this(adaptedObject)
        {
            Guard.ArgumentNotNull(readerWriterLock, "readerWriterLock");
            this.readerWriterLock = readerWriterLock;
        }

        public SafeEnumeratorDecorator(IEnumerator<T> adaptedObject, ReaderWriterLockSlim readerWriterLockSlim)
            : this(adaptedObject)
        {
            Guard.ArgumentNotNull(readerWriterLockSlim, "readerWriterLockSlim");
            this.readerWriterLockSlim = readerWriterLockSlim;
        }

        private SafeEnumeratorDecorator(IEnumerator<T> adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Current
        {
            get
            {
                this.CheckAndThrowDisposedException();
                return this.adaptedObject.Current;
            }
        }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                return this.adaptedObject.Current;
            }
        }

        #endregion Explicit Interface Properties

        #region Public Methods and Operators

        public bool MoveNext()
        {
            this.CheckAndThrowDisposedException();
            this.AcquireLock();
            bool canMoveNext = this.adaptedObject.MoveNext();
            if (!canMoveNext)
            {
                this.ReleaseLock();
            }

            return canMoveNext;
        }

        public void Reset()
        {
            this.CheckAndThrowDisposedException();
            this.adaptedObject.Reset();
            this.ReleaseLock();
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.ReleaseLock();
        }

        private void AcquireLock()
        {
            if (!this.hasLockAcquired)
            {
                if (this.readerWriterLock != null)
                {
                    this.readerWriterLock.AcquireReaderLock(Timeout.Infinite);
                }
                else
                {
                    this.readerWriterLockSlim.EnterReadLock();
                }

                this.hasLockAcquired = true;
            }
        }

        private void ReleaseLock()
        {
            if (this.hasLockAcquired)
            {
                if (this.readerWriterLock != null)
                {
                    this.readerWriterLock.ReleaseReaderLock();
                }
                else
                {
                    this.readerWriterLockSlim.ExitReadLock();
                }

                this.hasLockAcquired = false;
            }
        }

        #endregion Methods
    }
}
