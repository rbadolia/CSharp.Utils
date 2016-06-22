using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class SafeEnumerableDecorator<T> : IEnumerable<T>
    {
        #region Fields

        private readonly IEnumerable<T> adaptedObject;

        private readonly ReaderWriterLock readerWriterLock;

        private readonly ReaderWriterLockSlim readerWriterLockSlim;

        #endregion Fields

        #region Constructors and Finalizers

        public SafeEnumerableDecorator(IEnumerable<T> adaptedObject, ReaderWriterLock readerWriterLock)
            : this(adaptedObject)
        {
            Guard.ArgumentNotNull(readerWriterLock, "readerWriterLock");
            this.readerWriterLock = readerWriterLock;
        }

        public SafeEnumerableDecorator(IEnumerable<T> adaptedObject, ReaderWriterLockSlim readerWriterLockSlim)
            : this(adaptedObject)
        {
            Guard.ArgumentNotNull(readerWriterLockSlim, "readerWriterLockSlim");
            this.readerWriterLockSlim = readerWriterLockSlim;
        }

        private SafeEnumerableDecorator(IEnumerable<T> adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            if (this.readerWriterLock != null)
            {
                return new SafeEnumeratorDecorator<T>(this.adaptedObject.GetEnumerator(), this.readerWriterLock);
            }

            return new SafeEnumeratorDecorator<T>(this.adaptedObject.GetEnumerator(), this.readerWriterLockSlim);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion Explicit Interface Methods
    }
}
