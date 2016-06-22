using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Contracts;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Diagnostics.Performance;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Net
{
    public abstract class AbstractHttpListenerWrapper : AbstractDisposable, IInitializable
    {
        #region Fields

        private HttpListener _listener;

        private Thread _listeningThread;

        private long _numberOfRequestsProcessed;

        private long _numberOfRequestsReceived;

        private long _totalTicksTakenForProcessing;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractHttpListenerWrapper()
        {
            this.ListeningThreadPriority = ThreadPriority.Normal;
            this.ProcessingMode = ProcessingModes.Asynchronous;
            this.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            this.IsStarted = false;
            this.IsInitialized = false;
            this.Prefixes = new List<string>();
        }

        ~AbstractHttpListenerWrapper()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public AuthenticationSchemes AuthenticationSchemes { get; set; }

        [Display(Order = 4)]
        [PerfCounter(CounterTimeTypes = TimeTypes.Ticks | TimeTypes.Milliseconds | TimeTypes.Seconds)]
        public TimeSpan AverageTimeTakenPerRequest
        {
            get
            {
                long totalTicks = Interlocked.Read(ref this._totalTicksTakenForProcessing);
                long totalRequestsProcessed = Interlocked.Read(ref this._numberOfRequestsProcessed);
                if (totalRequestsProcessed > 0)
                {
                    totalTicks /= totalRequestsProcessed;
                }

                return TimeSpan.FromTicks(totalTicks);
            }
        }

        public bool IsInitialized { get; private set; }

        public bool IsStarted { get; private set; }

        public ThreadPriority ListeningThreadPriority { get; set; }

        [Display(Order = 1)]
        [PerfCounter]
        public long NumberOfRequestsProcessed
        {
            get
            {
                return Interlocked.Read(ref this._numberOfRequestsProcessed);
            }
        }

        [Display(Order = 0)]
        [PerfCounter]
        public long NumberOfRequestsReceived
        {
            get
            {
                return Interlocked.Read(ref this._numberOfRequestsReceived);
            }
        }

        [Display(Order = 2)]
        [PerfCounter]
        public long NumberOfRequestsUnderProcess
        {
            get
            {
                return Interlocked.Read(ref this._numberOfRequestsReceived) - Interlocked.Read(ref this._numberOfRequestsProcessed);
            }
        }

        public List<string> Prefixes { get; set; }

        public ProcessingModes ProcessingMode { get; set; }

        [Display(Order = 3)]
        [PerfCounter(CounterTimeTypes = TimeTypes.Ticks | TimeTypes.Milliseconds | TimeTypes.Seconds | TimeTypes.Minutes | TimeTypes.Hours)]
        public TimeSpan TotalTimeTakenInProcessing
        {
            get
            {
                return TimeSpan.FromTicks(this._totalTicksTakenForProcessing);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                this.InitializeProtected();
            }
        }

        public void Start()
        {
            if (this.IsInitialized && !this.IsStarted)
            {
                this.IsStarted = true;
                this.start();
            }
        }

        public void Stop()
        {
            if (this.IsStarted)
            {
                this.IsStarted = false;
                this.stop();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.Stop();
            if (this._listener != null)
            {
                this._listener.Close();
            }
        }

        protected virtual void InitializeProtected()
        {
            this._listener = new HttpListener { AuthenticationSchemes = this.AuthenticationSchemes };
            foreach (string prefix in this.Prefixes)
            {
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    this._listener.Prefixes.Add(prefix.Trim());
                }
            }
        }

        protected void listen()
        {
            if (!this._listener.IsListening)
            {
                this._listener.Start();
                bool isSuspended = false;
                try
                {
                    if (this.ProcessingMode == ProcessingModes.AsynchronousWithSuppressContext)
                    {
                        try
                        {
                            ExecutionContext.SuppressFlow();
                            isSuspended = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }

                    while (this._listener.IsListening)
                    {
                        try
                        {
                            HttpListenerContext context = this._listener.GetContext();
                            Interlocked.Increment(ref this._numberOfRequestsReceived);
                            long ticksBefore = SharedStopWatch.ElapsedTicks;
                            var pair = new Pair<HttpListenerContext, long>(context, ticksBefore);
                            if (this.ProcessingMode == ProcessingModes.Synchronous)
                            {
                                this.process1(pair);
                            }
                            else
                            {
                                Task.Factory.StartNew(this.process, pair);
                            }
                        }
                        catch (ThreadAbortException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
                finally
                {
                    if (this.ProcessingMode == ProcessingModes.AsynchronousWithSuppressContext && isSuspended)
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
        }

        protected virtual void listenThreadStart()
        {
            try
            {
                this.listen();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected virtual void process1(Pair<HttpListenerContext, long> pair)
        {
            try
            {
                this.processRequest(pair.First);
            }
            finally
            {
                Interlocked.Increment(ref this._numberOfRequestsProcessed);
                long elapsedTicks = SharedStopWatch.ElapsedTicks - pair.Second;
                Interlocked.Add(ref this._totalTicksTakenForProcessing, elapsedTicks);
            }
        }

        protected abstract void processRequest(HttpListenerContext context);

        protected virtual void start()
        {
            this._listeningThread = new Thread(this.listenThreadStart) { IsBackground = true, Priority = this.ListeningThreadPriority };
            this._listeningThread.Start();
        }

        protected virtual void stop()
        {
            try
            {
                this._listener.Stop();
                if (!this._listeningThread.Join(100))
                {
                    this._listeningThread.Abort();
                }

                this._listeningThread = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void process(object state)
        {
            var pair = state as Pair<HttpListenerContext, long>;
            this.process1(pair);
        }

        #endregion Methods
    }
}
