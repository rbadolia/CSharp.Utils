using System;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Threading
{
    public abstract class AbstractReaderWriterAtomicOperationSupported : AbstractDisposable, IAtomicOperationSupported
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected ReaderWriterLockSlim LockSlim
        {
            get
            {
                return this.readerWriterLockSlim;
            }
        }

        public void PerformAtomicOperation(Action action)
        {
            Guard.ArgumentNotNull(action, "action");
            try
            {
                this.LockSlim.EnterWriteLock();
                action();
            }
            finally
            {
                this.LockSlim.ExitWriteLock();
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.LockSlim.Dispose();
        }
    }
}
