using System;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class IdentityLockFlyweight : AbstractInitializable
    {
        private static IdentityLockFlyweight _instance = new IdentityLockFlyweight();

        private Dictionary<Guid, WeakReference> _dictionary = new Dictionary<Guid, WeakReference>();

        private ReaderWriterLockSlim _slimLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private long _cleanupInterval = 60000;

        private IntervalTask _cleanupTask;

        public long CleanupInterval
        {
            get { return this._cleanupInterval; }
            set
            {
                if (this.IsInitialized)
                {
                    throw new ObjectAlreadyInitializedException();
                }

                if (value < 1)
                {
                    throw new ArgumentException("CleanupInterval should be >0");
                }

                this._cleanupInterval = value;
            }
        }

        private IdentityLockFlyweight()
        {

        }

        public static IdentityLockFlyweight Instance
        {
            get { return _instance; }
        }

        public object GetLock(Guid id)
        {
            if (!this.IsInitialized)
            {
                this.Initialize();
            }

            try
            {
                this._slimLock.EnterUpgradeableReadLock();
                WeakReference reference;
                if (this._dictionary.TryGetValue(id, out reference))
                {
                    object target = reference.Target;
                    if (target == null)
                    {

                        try
                        {
                            this._slimLock.EnterWriteLock();
                            target = new object();
                            reference.Target = target;
                        }
                        finally
                        {
                            this._slimLock.ExitWriteLock();
                        }
                    }

                    return target;
                }
                else
                {
                    try
                    {
                        this._slimLock.EnterWriteLock();
                        var target = new object();
                        this._dictionary.Add(id, new WeakReference(target));
                        return target;
                    }
                    finally
                    {
                        this._slimLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this._slimLock.ExitUpgradeableReadLock();
            }
        }

        protected override void InitializeProtected()
        {
            this._cleanupTask = new IntervalTask(this.CleanupInterval, false);
            this._cleanupTask.AddAction(this.CleanupDictionary, null);
            this._cleanupTask.Start();
        }

        private void CleanupDictionary(object tag)
        {
            try
            {
                this._cleanupTask.Stop();
                this._slimLock.EnterWriteLock();
                var list = new List<Guid>();
                foreach (var kvp in this._dictionary)
                {
                    object target = kvp.Value.Target;
                    if (target == null)
                    {
                        list.Add(kvp.Key);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    this._dictionary.Remove(list[i]);
                }
            }
            finally
            {
                this._slimLock.ExitWriteLock();
                this._cleanupTask.Start();
            }
        }
    }
}
