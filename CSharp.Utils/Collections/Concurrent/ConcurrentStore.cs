using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CSharp.Utils.Diagnostics;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class ConcurrentStore<T> : IConcurrentStore<T>
    {
        #region Constants

        private const int WaitTime = 10;

        #endregion Constants

        #region Fields

        private readonly StoreFlushEventArgs<T>[] args = new StoreFlushEventArgs<T>[2];

        private readonly int bufferSize;

        private readonly T[][] buffers = new T[2][];

        private readonly AutoResetEvent flushEvent = new AutoResetEvent(false);

        private readonly ThreadPriority flushingThreadPriority = ThreadPriority.Normal;

        private readonly AutoResetEvent forceFlushEvent = new AutoResetEvent(true);

        private readonly bool isReferenceType;

        private readonly long maxWaitTimeInTicks = -1;

        private readonly ManualResetEvent[] writeEvents = new ManualResetEvent[2];

        private readonly int[] writingIndexes = new int[2];

        private readonly int[] writtenCounts = new int[2];

        private readonly T defaultT = default(T);

        private int disposeCount;

        private long flushStartedAt = -1;

        private Thread flushingThread;

        private bool isDisposeCalled;

        private bool isInitialized;

        private int lastFlushIndex = -1;

        private int writingBufferIndex;

        #endregion Fields

        #region Constructors and Finalizers

        public ConcurrentStore(int bufferSize, int periodicFlushInterval)
            : this(bufferSize, ThreadPriority.Normal, periodicFlushInterval)
        {
        }

        public ConcurrentStore(int bufferSize, ThreadPriority flushingThreadPriority, int periodicFlushInterval)
            : this(bufferSize, flushingThreadPriority, periodicFlushInterval, -1)
        {
        }

        public ConcurrentStore(int bufferSize, ThreadPriority flushingThreadPriority, int periodicFlushInterval, long maxWaitTime)
        {
            this.maxWaitTimeInTicks = maxWaitTime * TimeSpan.TicksPerMillisecond;
            this.isReferenceType = !typeof(T).IsValueType;
            this.PeriodicFlushInterval = periodicFlushInterval;
            this.flushingThreadPriority = flushingThreadPriority;
            this.writingIndexes[0] = -1;
            this.writingIndexes[1] = -1;

            this.writtenCounts[0] = 0;
            this.writtenCounts[1] = 0;

            this.bufferSize = bufferSize;
            this.writeEvents[0] = new ManualResetEvent(false);
            this.writeEvents[1] = new ManualResetEvent(true);
            this.ClearBufferAfterFlush = false;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<StoreFlushEventArgs<T>> OnStoreFlush;

        #endregion Public Events

        #region Public Properties

        public bool ClearBufferAfterFlush { get; set; }

        public string FlushingThreadName
        {
            get
            {
                return this.flushingThread.Name;
            }

            set
            {
                this.flushingThread.Name = value;
            }
        }

        public long MaxWaitTime
        {
            get
            {
                return this.maxWaitTimeInTicks / TimeSpan.TicksPerMillisecond;
            }
        }

        public int PeriodicFlushInterval { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool Add(T item)
        {
            return this.CheckAndAdd(item);
        }

        public void Dispose()
        {
            int disposeCountLocal = Interlocked.Increment(ref this.disposeCount);
            if (disposeCountLocal == 1)
            {
                this.ForceFlush(true);
            }
        }

        public void ForceFlush()
        {
            this.ForceFlush(false);
        }

        #endregion Public Methods and Operators

        #region Methods

        private bool CheckAndAdd(T item)
        {
            bool b = this.isInitialized;
            Thread.MemoryBarrier();
            if (!b)
            {
                lock (this.buffers)
                {
                    b = this.isInitialized;
                    Thread.MemoryBarrier();
                    if (!b)
                    {
                        this.buffers[0] = new T[this.bufferSize];
                        this.buffers[1] = new T[this.bufferSize];
                        this.args[0] = new StoreFlushEventArgs<T>(this.buffers[0]);
                        this.args[1] = new StoreFlushEventArgs<T>(this.buffers[1]);

                        this.flushingThread = new Thread(this.FlushBuffer) { Priority = this.flushingThreadPriority, IsBackground = true };
                        this.flushingThread.Start();

                        Thread.MemoryBarrier();
                        this.isInitialized = true;
                        Thread.MemoryBarrier();
                    }
                }
            }

            return this.performAdd(item);
        }

        private void ClearAllBuffers()
        {
            this.ClearBuffer(this.buffers[0], 0, this.bufferSize - 1);
            this.ClearBuffer(this.buffers[1], 0, this.bufferSize - 1);
        }

        private void ClearBuffer(IList<T> buffer, int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                buffer[i] = this.defaultT;
            }
        }

        private void DoFlush()
        {
            try
            {
                int currentWritingBufferIndex = this.writingBufferIndex;
                Thread.MemoryBarrier();
                int wc = this.writtenCounts[currentWritingBufferIndex];
                int lfi = this.lastFlushIndex;
                if (wc == this.bufferSize)
                {
                    int newWritingBufferIndex = currentWritingBufferIndex == 0 ? 1 : 0;
                    this.writeEvents[newWritingBufferIndex].Reset();
                    this.lastFlushIndex = -1;
                    this.writtenCounts[newWritingBufferIndex] = 0;
                    this.writingIndexes[newWritingBufferIndex] = -1;
                    Thread.MemoryBarrier();
                    this.writingBufferIndex = newWritingBufferIndex;
                    this.writeEvents[currentWritingBufferIndex].Set();
                }
                else
                {
                    this.lastFlushIndex = wc - 1;
                }

                if (this.OnStoreFlush != null)
                {
                    try
                    {
                        bool isFlushed = false;
                        if (wc - lfi == this.bufferSize + 1)
                        {
                            this.OnStoreFlush(this, this.args[currentWritingBufferIndex]);
                            isFlushed = true;
                        }
                        else
                        {
                            if (wc - lfi >= 2)
                            {
                                this.OnStoreFlush(this, new StoreFlushEventArgs<T>(this.buffers[currentWritingBufferIndex], lfi + 1, wc - 1));
                                isFlushed = true;
                            }
                        }

                        if (this.ClearBufferAfterFlush && isFlushed && this.isReferenceType)
                        {
                            this.ClearBuffer(this.buffers[currentWritingBufferIndex], lfi + 1, wc - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void FlushBuffer()
        {
            while (true)
            {
                this.flushStartedAt = -1;
                Thread.MemoryBarrier();
                this.flushEvent.WaitOne(this.PeriodicFlushInterval);
                this.flushStartedAt = SharedStopWatch.ElapsedTicks;
                Thread.MemoryBarrier();
                this.forceFlushEvent.Reset();
                this.DoFlush();
                this.flushStartedAt = -1;
                this.forceFlushEvent.Set();
                Thread.MemoryBarrier();
                bool disposedCalled = this.isDisposeCalled;
                Thread.MemoryBarrier();
                if (disposedCalled)
                {
                    if (this.isReferenceType)
                    {
                        this.ClearAllBuffers();
                    }

                    break;
                }
            }
        }

        private void ForceFlush(bool isDispose)
        {
            this.forceFlushEvent.WaitOne();
            if (isDispose)
            {
                this.isDisposeCalled = true;
            }

            this.flushEvent.Set();
            this.forceFlushEvent.WaitOne();
        }

        private bool performAdd(T item)
        {
            int wi;
            int index;
            while (true)
            {
                Thread.MemoryBarrier();
                wi = this.writingBufferIndex;
                index = Interlocked.Increment(ref this.writingIndexes[wi]);
                if (index >= this.bufferSize || index < 0)
                {
                    if (this.MaxWaitTime > -1)
                    {
                        Thread.MemoryBarrier();
                        long l = this.flushStartedAt;
                        if (l != -1 && SharedStopWatch.ElapsedTicks - l > this.maxWaitTimeInTicks)
                        {
                            return false;
                        }
                    }

                    this.writeEvents[wi].WaitOne(WaitTime);
                }
                else
                {
                    break;
                }
            }

            this.buffers[wi][index] = item;
            int wc = Interlocked.Increment(ref this.writtenCounts[wi]);
            if (wc == this.bufferSize)
            {
                this.flushEvent.Set();
            }

            return true;
        }

        #endregion Methods
    }
}
