using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Utils.Threading
{
    public sealed class SemaphoreWrapper : AbstractDisposable
    {
        #region Fields

        private readonly Semaphore _semaphore;

        #endregion Fields

        #region Constructors and Finalizers

        public SemaphoreWrapper(int initialCount, int maximumCount)
        {
            this._semaphore = new Semaphore(initialCount, maximumCount);
        }

        ~SemaphoreWrapper()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public long QueueupItems(IEnumerable enumerable, WaitCallback callback, bool returnOnlyAfterCompletingAll, bool suppressContext = false)
        {
            this.CheckAndThrowDisposedException();
            bool isContextSupressed = false;
            try
            {
                if (suppressContext)
                {
                    try
                    {
                        ExecutionContext.SuppressFlow();
                        isContextSupressed = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                var pendingCount = new PendingCount();
                long count = 0;
                foreach (object obj in enumerable)
                {
                    while (true)
                    {
                        if (this.IsDisposed)
                        {
                            return count;
                        }

                        if (this._semaphore.WaitOne(500))
                        {
                            count++;

                            Task.Factory.StartNew(state =>
                                {
                                    try
                                    {
                                        pendingCount.Increment();
                                        callback(state);
                                    }
                                    finally
                                    {
                                        this._semaphore.Release();
                                        pendingCount.Decrement();
                                    }
                                }, obj);
                            break;
                        }

                        Thread.Yield();
                    }
                }

                if (returnOnlyAfterCompletingAll)
                {
                    while (pendingCount.Count != 0)
                    {
                        Thread.Yield();
                    }
                }

                return count;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
            finally
            {
                if (suppressContext && isContextSupressed)
                {
                    try
                    {
                        ExecutionContext.RestoreFlow();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this._semaphore.Dispose();
        }

        #endregion Methods

        public class PendingCount
        {
            #region Fields

            private long _pendingCount;

            #endregion Fields

            #region Public Properties

            public long Count
            {
                get
                {
                    return Interlocked.Read(ref this._pendingCount);
                }
            }

            #endregion Public Properties

            #region Public Methods and Operators

            public void Decrement()
            {
                Interlocked.Decrement(ref this._pendingCount);
            }

            public void Increment()
            {
                Interlocked.Increment(ref this._pendingCount);
            }

            #endregion Public Methods and Operators
        }
    }
}
