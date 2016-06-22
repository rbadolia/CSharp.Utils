using System;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Threading
{
    public abstract class AbstractMonitorAtomicOperationSupported : IAtomicOperationSupported
    {
        private readonly object syncLockObject = new object();

        protected object SyncLockObject
        {
            get
            {
                return this.syncLockObject;
            }
        }

        public void PerformAtomicOperation(Action action)
        {
            Guard.ArgumentNotNull(action, "action");
            lock (this.syncLockObject)
            {
                action();
            }
        }
    }
}
