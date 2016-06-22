using System;
using System.Threading;

namespace CSharp.Utils.Collections.Generic
{

    #region Delegates

    public delegate T GetCacheObjectCallback<out T>();

    #endregion Delegates

    public sealed class SimpleTimedCache<T> : AbstractDisposable
    {
        #region Fields

        private readonly GetCacheObjectCallback<T> callback;

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        private T cacheObject;

        private DateTime? lastCreationTime;

        #endregion Fields

        #region Constructors and Finalizers

        public SimpleTimedCache(long interval, GetCacheObjectCallback<T> callback)
        {
            this.Interval = interval;
            this.callback = callback;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public long Interval { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public T GetObject()
        {
            this.readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (this.ShouldRefreshCache())
                {
                    this.readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        if (this.ShouldRefreshCache())
                        {
                            this.refreshCache();
                        }
                    }
                    finally
                    {
                        this.readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this.readerWriterLockSlim.ExitUpgradeableReadLock();
            }

            return this.cacheObject;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.readerWriterLockSlim.Dispose();
        }

        private void refreshCache()
        {
            this.cacheObject = this.callback();
            this.lastCreationTime = GlobalSettings.Instance.CurrentDateTime;
        }

        private bool ShouldRefreshCache()
        {
            if (this.lastCreationTime == null)
            {
                return true;
            }

            if ((GlobalSettings.Instance.CurrentDateTime - this.lastCreationTime.Value).TotalMilliseconds >= this.Interval)
            {
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}
