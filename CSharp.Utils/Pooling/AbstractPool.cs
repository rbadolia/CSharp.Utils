using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CSharp.Utils.Pooling.Internal;

namespace CSharp.Utils.Pooling
{

    #region Delegates

    public delegate void ReturnAdaptedObjectToPoolDelegate<AT>(IPoolItem<AT> poolItem, AT adaptedObject);

    #endregion Delegates

    public abstract class AbstractPool<TAdaptedObject, TPoolItem>
        where TPoolItem : IPoolItem<TAdaptedObject>, new()
    {
        #region Static Fields

        private static readonly bool _isAdaptedObjectDisposableType = typeof(IDisposable).IsAssignableFrom(typeof(TAdaptedObject));

        private static int _poolIdSequencer;

        #endregion Static Fields

        #region Fields

        private readonly AutoResetEvent _clearingThreadWakeupWaitHandle = new AutoResetEvent(false);

        private readonly AutoResetEvent _objectReturnedToPoolWaitHandle = new AutoResetEvent(false);

        private readonly int _poolId;

        private readonly Dictionary<TAdaptedObject, PoolItemInfo> _poolItemsDictionary = new Dictionary<TAdaptedObject, PoolItemInfo>();

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private Thread _clearingThread;

        private Semaphore _countControllingSemaphore;

        private WaitHandle[] _getObjectWaitHandles;

        private int _maxPoolSize = int.MaxValue;

        private int _objectMaxIdleTimeInMilliseconds = int.MaxValue;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractPool(int maxPoolSize, int objectMaxIdleTimeInMilliseconds, bool canObjectBeShared)
            : this()
        {
            this.MaxPoolSize = maxPoolSize;
            this.ObjectMaxIdleTimeInMilliseconds = objectMaxIdleTimeInMilliseconds;
            this.CanObjectBeShared = canObjectBeShared;
        }

        protected AbstractPool()
        {
            this._poolId = Interlocked.Increment(ref _poolIdSequencer);
        }

        #endregion Constructors and Finalizers

        #region Delegates

        public delegate bool CanSelectThisAdaptedObjectDelegate(TAdaptedObject adaptedObject, bool isNewlyCreatedAdaptedObject, object tag);

        #endregion Delegates

        #region Public Properties

        public bool CanObjectBeShared { get; set; }

        public int Count
        {
            get
            {
                try
                {
                    this._rwLock.EnterReadLock();
                    return this._poolItemsDictionary.Count;
                }
                finally
                {
                    this._rwLock.ExitReadLock();
                }
            }
        }

        public bool IsInitialized { get; private set; }

        public int MaxPoolSize
        {
            get
            {
                return this._maxPoolSize;
            }

            set
            {
                if (value != this._maxPoolSize)
                {
                    if (this.IsInitialized)
                    {
                        throw new ObjectAlreadyInitializedException("MaxPoolSize cannot be changed after the Pool is initialized");
                    }

                    if (value <= 0)
                    {
                        throw new ArgumentException(@"MaxPoolSize should be greater than zero", "value");
                    }

                    this._maxPoolSize = value;
                }
            }
        }

        public int ObjectMaxIdleTimeInMilliseconds
        {
            get
            {
                return this._objectMaxIdleTimeInMilliseconds;
            }

            set
            {
                if (value != this._objectMaxIdleTimeInMilliseconds)
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException(@"ObjectMaxIdleTimeInMilliseconds should be greater than zero", "value");
                    }

                    this._objectMaxIdleTimeInMilliseconds = value;
                    this._clearingThreadWakeupWaitHandle.Set();
                }
            }
        }

        #endregion Public Properties

        #region Methods

        protected IAsyncResult beginGetObject(CanSelectThisAdaptedObjectDelegate callback, object tag = null)
        {
            var result = new PoolAsyncResult(this._poolId, this.GetType());
            Task.Factory.StartNew(delegate
                {
                    TPoolItem obj = this.getObject(callback, tag);
                    result.SetCompleted(obj);
                });
            return result;
        }

        protected IAsyncResult beginTryGetObject(CanSelectThisAdaptedObjectDelegate callback, int millisecondsToWait, object tag = null)
        {
            var result = new PoolAsyncResult(this._poolId, this.GetType());
            Task.Factory.StartNew(() =>
                {
                    TPoolItem obj;
                    bool hasGot = this.tryGetObject(out obj, callback, millisecondsToWait, tag);
                    result.SetCompleted(new KeyValuePair<bool, TPoolItem>(hasGot, obj));
                });
            return result;
        }

        protected virtual bool canSelectThisAdaptedObject(TAdaptedObject adaptedObject, bool isNewlyCreatedAdaptedObject, object tag)
        {
            return true;
        }

        protected virtual void cleanupAdaptedObject(TAdaptedObject adaptedObject)
        {
            if (_isAdaptedObjectDisposableType)
            {
                ((IDisposable)adaptedObject).Dispose();
            }
        }

        protected TPoolItem endGetObject(IAsyncResult result)
        {
            this.validateAsyncResult(result);
            result.AsyncWaitHandle.WaitOne();
            return (TPoolItem)result.AsyncState;
        }

        protected bool endTryGetObject(IAsyncResult result, out TPoolItem obj)
        {
            this.validateAsyncResult(result);
            result.AsyncWaitHandle.WaitOne();
            var kvp = (KeyValuePair<bool, TPoolItem>)result.AsyncState;
            obj = kvp.Value;
            return kvp.Key;
        }

        protected abstract TAdaptedObject getAdaptedObject();

        protected TPoolItem getObject(CanSelectThisAdaptedObjectDelegate callback, object tag = null)
        {
            if (callback == null)
            {
                callback = this.canSelectThisAdaptedObject;
            }

            this.checkAndInitialize();
            TPoolItem adaptedObject;
            while (!this.tryGetObjectPrivate(out adaptedObject, callback, 100, tag))
            {
            }

            return adaptedObject;
        }

        protected bool tryGetObject(out TPoolItem adaptedObject, CanSelectThisAdaptedObjectDelegate callback, int millisecondsToWait, object tag = null)
        {
            if (millisecondsToWait < 0)
            {
                throw new ArgumentException(@"The argument value should be greater than -1", "millisecondsToWait");
            }

            if (callback == null)
            {
                callback = this.canSelectThisAdaptedObject;
            }

            this.checkAndInitialize();
            return this.tryGetObjectPrivate(out adaptedObject, callback, millisecondsToWait, tag);
        }

        private void checkAndInitialize()
        {
            if (!this.IsInitialized)
            {
                lock (this._poolItemsDictionary)
                {
                    if (!this.IsInitialized)
                    {
                        this.IsInitialized = true;
                        this._countControllingSemaphore = new Semaphore(this.MaxPoolSize, this.MaxPoolSize);
                        this._getObjectWaitHandles = new WaitHandle[] { this._countControllingSemaphore, this._objectReturnedToPoolWaitHandle };
                        this._clearingThread = new Thread(this.clearingThreadStart) { IsBackground = true };
                        this._clearingThread.Start();
                    }
                }
            }
        }

        private void cleanupAdaptedObjects(IEnumerable<TAdaptedObject> adaptedObjects)
        {
            foreach (TAdaptedObject adaptedObject in adaptedObjects)
            {
                try
                {
                    this.cleanupAdaptedObject(adaptedObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private void clearingThreadStart()
        {
            try
            {
                while (true)
                {
                    DateTime startedDateTime = DateTime.Now;
                    TimeSpan objectMaxLifeTime = TimeSpan.FromMilliseconds(this.ObjectMaxIdleTimeInMilliseconds);
                    TimeSpan minTimeSpan = objectMaxLifeTime;
                    int maximumPoolSize = this.MaxPoolSize;
                    var adaptedObjectsToCleanup = new List<TAdaptedObject>();
                    try
                    {
                        this._rwLock.EnterUpgradeableReadLock();
                        int i = 0;
                        foreach (var kvp in this._poolItemsDictionary)
                        {
                            PoolItemInfo info = kvp.Value;
                            if (Interlocked.Read(ref info.usageCount) == 0)
                            {
                                TimeSpan objectRemainingLifeTime = objectMaxLifeTime - (startedDateTime - info.lastUsedOn);
                                if (objectRemainingLifeTime < TimeSpan.Zero || i >= maximumPoolSize)
                                {
                                    adaptedObjectsToCleanup.Add(kvp.Key);
                                }
                                else
                                {
                                    if (objectRemainingLifeTime < minTimeSpan)
                                    {
                                        minTimeSpan = objectRemainingLifeTime;
                                    }
                                }
                            }

                            i++;
                        }

                        if (adaptedObjectsToCleanup.Count > 0)
                        {
                            try
                            {
                                i = 0;
                                this._rwLock.EnterWriteLock();
                                for (; i < adaptedObjectsToCleanup.Count; i++)
                                {
                                    this._poolItemsDictionary.Remove(adaptedObjectsToCleanup[i]);
                                    this._countControllingSemaphore.Release();
                                }
                            }
                            finally
                            {
                                this._rwLock.ExitWriteLock();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        this._rwLock.ExitUpgradeableReadLock();
                        this.cleanupAdaptedObjects(adaptedObjectsToCleanup);
                    }

                    var timeToSleep = (int)(minTimeSpan - (DateTime.Now - startedDateTime)).TotalMilliseconds;
                    this._clearingThreadWakeupWaitHandle.WaitOne(Math.Max(timeToSleep, 1));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void returnObjectToPool(IPoolItem<TAdaptedObject> poolItem, TAdaptedObject adaptedObject)
        {
            try
            {
                this._rwLock.EnterReadLock();
                PoolItemInfo poolItemInfo;
                if (this._poolItemsDictionary.TryGetValue(adaptedObject, out poolItemInfo))
                {
                    if (Interlocked.Decrement(ref poolItemInfo.usageCount) == 0)
                    {
                        poolItemInfo.lastUsedOn = DateTime.Now;
                        poolItem.SetAdaptedObjectAndCallback(default(TAdaptedObject), this.returnObjectToPool);
                        this._objectReturnedToPoolWaitHandle.Set();
                    }
                }
            }
            finally
            {
                this._rwLock.ExitReadLock();
            }
        }

        private bool tryGetObjectPrivate(out TPoolItem adaptedObject, CanSelectThisAdaptedObjectDelegate callback, int millisecondsToWait, object tag)
        {
            adaptedObject = default(TPoolItem);
            DateTime startedTime = DateTime.Now;
            do
            {
                try
                {
                    this._rwLock.EnterUpgradeableReadLock();
                    PoolItemInfo selectedItem = null;
                    TAdaptedObject selectedAdaptedObject = default(TAdaptedObject);
                    foreach (var kvp in this._poolItemsDictionary)
                    {
                        if (this.CanObjectBeShared)
                        {
                            if (callback(kvp.Key, false, tag))
                            {
                                selectedAdaptedObject = kvp.Key;
                                selectedItem = kvp.Value;
                                break;
                            }
                        }
                        else
                        {
                            if (Interlocked.Read(ref kvp.Value.usageCount) == 0)
                            {
                                if (callback(kvp.Key, false, tag))
                                {
                                    selectedAdaptedObject = kvp.Key;
                                    selectedItem = kvp.Value;
                                    break;
                                }
                            }
                        }
                    }

                    if (selectedItem == null)
                    {
                        this._objectReturnedToPoolWaitHandle.Reset();
                        int index = WaitHandle.WaitAny(this._getObjectWaitHandles, millisecondsToWait);

                        if (index < 0)
                        {
                            return false;
                        }

                        if (index == 0)
                        {
                            try
                            {
                                this._rwLock.EnterWriteLock();
                                TAdaptedObject a = this.getAdaptedObject();
                                callback(a, true, tag);
                                selectedItem = new PoolItemInfo();
                                selectedAdaptedObject = a;
                                this._poolItemsDictionary.Add(selectedAdaptedObject, selectedItem);
                            }
                            finally
                            {
                                this._rwLock.ExitWriteLock();
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    Interlocked.Increment(ref selectedItem.usageCount);
                    var pt = new TPoolItem();
                    pt.SetAdaptedObjectAndCallback(selectedAdaptedObject, this.returnObjectToPool);
                    adaptedObject = pt;
                    return true;
                }
                finally
                {
                    this._rwLock.ExitUpgradeableReadLock();
                    millisecondsToWait -= (int)(DateTime.Now - startedTime).TotalMilliseconds;
                }
            }
            while (millisecondsToWait > 0);
            return false;
        }

        private void validateAsyncResult(IAsyncResult result)
        {
            var pResult = result as PoolAsyncResult;
            if (pResult == null || pResult.PoolId != this._poolId || pResult.PoolType != this.GetType())
            {
                throw new ArgumentException(@"The IAsyncResult that is passed is not created by this pool", "result");
            }
        }

        #endregion Methods

        private sealed class PoolItemInfo
        {
            #region Fields

            internal DateTime lastUsedOn;

            internal long usageCount;

            #endregion Fields
        }
    }
}
