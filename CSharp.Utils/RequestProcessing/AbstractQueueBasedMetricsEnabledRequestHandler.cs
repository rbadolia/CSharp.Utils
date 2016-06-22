using System;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.RequestProcessing
{
    public abstract class AbstractQueueBasedRequestHandler<TRequest> : AbstractRequestHandler<TRequest>, IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        #region Constants

        private const int DisposeCheckInterval = 1000;

        #endregion Constants

        #region Fields

        private Thread _dequeuingThread;

        private AutoResetEvent _itemWrittenIntoQueueWaitHandle;

        private ISimpleQueue<TRequest> _queue;

        private int? _sleepTimeInSeconds;

        #endregion Fields

        #region Public Properties

        public ISimpleQueue<TRequest> Queue
        {
            get
            {
                return this._queue;
            }

            set
            {
                if (this.IsInitialized && value != this._queue)
                {
                    throw new ObjectAlreadyInitializedException("Queue cannot be changed after the object is initialized");
                }

                this._queue = value;
            }
        }

        public int? SleepTimeInSeconds
        {
            get
            {
                return this._sleepTimeInSeconds;
            }

            set
            {
                if (this.IsInitialized && value != this.SleepTimeInSeconds)
                {
                    throw new ObjectAlreadyInitializedException("SleepTimeInSeconds cannot be changed after the object is initialized");
                }

                if (value == null)
                {
                    this._sleepTimeInSeconds = null;
                }
                else
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("SleepTimeInSeconds should be greater than 0", "SleepTimeInSeconds");
                    }

                    this._sleepTimeInSeconds = value;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public override IResponse ProcessRequest(TRequest request)
        {
            if (this.Queue == null)
            {
                return base.ProcessRequest(request);
            }

            this.Queue.Enqueue(request);
            if (this.SleepTimeInSeconds == null)
            {
                this._itemWrittenIntoQueueWaitHandle.Set();
            }

            return new GenericResponse { RequestId = request.RequestId };
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void InitializeProtected()
        {
            if (this.Queue != null)
            {
                if (this.SleepTimeInSeconds == null)
                {
                    this._itemWrittenIntoQueueWaitHandle = new AutoResetEvent(false);
                }

                this._dequeuingThread = new Thread(this.dequeuingThreadStart) { IsBackground = true };
                this._dequeuingThread.Start();
            }
        }

        private void dequeuingThreadStart()
        {
            while (true)
            {
                IEnumerable<TRequest> objects = this.Queue.Dequeue();
                int count = 0;
                foreach (TRequest obj in objects)
                {
                    base.ProcessRequest(obj);
                    count++;
                }

                if (this.IsDisposed)
                {
                    return;
                }

                if (count == 0)
                {
                    if (this.SleepTimeInSeconds != null)
                    {
                        for (int i = 0; i < this.SleepTimeInSeconds.Value; i++)
                        {
                            if (this.IsDisposed)
                            {
                                return;
                            }

                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            if (this.IsDisposed)
                            {
                                return;
                            }

                            this._itemWrittenIntoQueueWaitHandle.WaitOne(DisposeCheckInterval);
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}
