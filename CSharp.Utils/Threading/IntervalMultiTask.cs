using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using CSharp.Utils.Diagnostics;
using Timer = System.Timers.Timer;

namespace CSharp.Utils.Threading
{

    #region Delegates

    public delegate void ActionDelegate(object tag);

    #endregion Delegates

    public sealed class IntervalMultiTask : AbstractDisposable
    {
        #region Static Fields

        private static readonly object _syncLock = new object();

        private static IntervalMultiTask _defaultInstance;

        #endregion Static Fields

        #region Fields

        private readonly List<ActionInfo> _actions = new List<ActionInfo>();

        private readonly AutoResetEvent _event = new AutoResetEvent(false);

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly bool _useDedicatedThread = true;

        private string _invokingThreadName;

        private bool _isStopped;

        private Thread _thread;

        private Timer _timer;

        #endregion Fields

        #region Constructors and Finalizers

        public IntervalMultiTask(bool useDedicatedThread)
        {
            this._useDedicatedThread = useDedicatedThread;
        }

        ~IntervalMultiTask()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static IntervalMultiTask DefaultInstance
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (_syncLock)
                    {
                        if (_defaultInstance == null)
                        {
                            _defaultInstance = new IntervalMultiTask(true);
                        }
                    }
                }

                return _defaultInstance;
            }
        }

        public string InvokingThreadName
        {
            get
            {
                return this._invokingThreadName;
            }

            set
            {
                this._invokingThreadName = value;
                if (this._thread != null)
                {
                    this._thread.Name = value;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void AddAction(ActionDelegate action, long interval, object tag)
        {
            this._lock.EnterWriteLock();
            try
            {
                long intervalInTicks = interval * TimeSpan.TicksPerMillisecond;
                this._actions.Add(new ActionInfo(action, intervalInTicks, SharedStopWatch.ElapsedTicks, tag));
                if (this._actions.Count == 1)
                {
                    this._isStopped = false;
                    this.start(interval);
                }
                else
                {
                    if (this._useDedicatedThread)
                    {
                        this._event.Set();
                    }
                    else
                    {
                        this._timer.Stop();
                        this._timer.Interval = 1;
                        this._timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void RemoveAction(ActionDelegate action, object tag)
        {
            this._lock.EnterWriteLock();
            try
            {
                for (int i = 0; i < this._actions.Count; i++)
                {
                    if (this._actions[i].Actiondelegate == action && this._actions[i].Tag == tag)
                    {
                        this._actions.RemoveAt(i);
                        i--;
                    }
                }

                if (this._actions.Count == 0)
                {
                    this._isStopped = true;
                }
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this != _defaultInstance)
            {
                this._isStopped = true;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._timer.Stop();
            Thread.CurrentThread.Name = this._invokingThreadName;
            long ticksBefore = SharedStopWatch.ElapsedTicks;
            long milliSecondsToWait = this.performActions();
            if (!this._isStopped)
            {
                this._timer.Interval = milliSecondsToWait > 0 ? milliSecondsToWait : 1;
                this._timer.Start();
            }
            else
            {
                this._timer.Dispose();
            }
        }

        private long performActions()
        {
            this._lock.EnterReadLock();
            try
            {
                foreach (ActionInfo v in this._actions)
                {
                    long diff = SharedStopWatch.ElapsedTicks - v.LastInvokedTime;
                    if (diff >= v.IntervalInTicks)
                    {
                        try
                        {
                            v.LastInvokedTime = SharedStopWatch.ElapsedTicks;
                            v.Actiondelegate(v.Tag);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }

                long maxTicksToWait = long.MaxValue;
                for (int i = 0; i < this._actions.Count; i++)
                {
                    ActionInfo action = this._actions[i];
                    long ticks = action.IntervalInTicks - (SharedStopWatch.ElapsedTicks - action.LastInvokedTime);
                    if (ticks < maxTicksToWait)
                    {
                        maxTicksToWait = ticks;
                    }
                }

                long millisecondsToWait = maxTicksToWait <= 0 ? 0 : (maxTicksToWait / TimeSpan.TicksPerMillisecond);
                return millisecondsToWait;
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        private void start(long sleepInterval)
        {
            if (!this._useDedicatedThread)
            {
                this._timer = new Timer(sleepInterval);
                this._timer.Elapsed += this._timer_Elapsed;
                this._timer.AutoReset = false;
                this._timer.Start();
            }
            else
            {
                this._thread = new Thread(delegate(object state)
                    {
                        try
                        {
                            var interval = (long)state;
                            Thread.Sleep((int)interval);
                            while (!this._isStopped)
                            {
                                long ticksBefore = SharedStopWatch.ElapsedTicks;
                                long milliSecondsToWait = this.performActions();
                                if (milliSecondsToWait > 0)
                                {
                                    this._event.WaitOne((int)milliSecondsToWait);
                                    this._event.Reset();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    })
                    {
                        Name = this._invokingThreadName, 
                        IsBackground = true
                    };
                this._thread.Start(sleepInterval);
            }
        }

        #endregion Methods

        private sealed class ActionInfo
        {
            #region Constructors and Finalizers

            public ActionInfo(ActionDelegate actiondelegate, long intervalInTicks, long lastInvokedTime, object tag)
            {
                this.Actiondelegate = actiondelegate;
                this.IntervalInTicks = intervalInTicks;
                this.LastInvokedTime = lastInvokedTime;
                this.Tag = tag;
            }

            #endregion Constructors and Finalizers

            #region Public Properties

            public ActionDelegate Actiondelegate { get; private set; }

            public long IntervalInTicks { get; private set; }

            public long LastInvokedTime { get; set; }

            public object Tag { get; private set; }

            #endregion Public Properties
        }
    }
}
