using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CSharp.Utils.Diagnostics;
using Timer = System.Timers.Timer;

namespace CSharp.Utils.Threading
{
    public sealed class IntervalTask : AbstractDisposable
    {
        #region Fields

        private readonly List<KeyValuePair<ActionDelegate, object>> _actions = new List<KeyValuePair<ActionDelegate, object>>();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly bool _strictInterval;

        private readonly bool _useDedicatedThread = true;

        private long _interval;

        private string _invokingThreadName;

        private bool _isStopped = true;

        private Thread _thread;

        private Timer _timer;

        #endregion Fields

        #region Constructors and Finalizers

        public IntervalTask(long interval, bool useDedicatedThread)
            : this(interval, useDedicatedThread, false)
        {
        }

        public IntervalTask(long interval, bool useDedicatedThread, bool strictInterval)
        {
            this._interval = interval;
            this._useDedicatedThread = useDedicatedThread;
            this._strictInterval = strictInterval;
        }

        ~IntervalTask()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public long Interval
        {
            get
            {
                return this._interval;
            }

            set
            {
                this._interval = value;
                if (this._timer != null)
                {
                    this._timer.Interval = value;
                }
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

        public bool IsStarted
        {
            get
            {
                return !this._isStopped;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void AddAction(ActionDelegate action, object tag)
        {
            this._lock.EnterWriteLock();
            try
            {
                this._actions.Add(new KeyValuePair<ActionDelegate, object>(action, tag));
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
                    if (this._actions[i].Key == action && this._actions[i].Value == tag)
                    {
                        this._actions.RemoveAt(i);
                        i--;
                    }
                }
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Start()
        {
            if (this._isStopped)
            {
                this._isStopped = false;
                if (!this._useDedicatedThread)
                {
                    this._timer = new Timer(this.Interval);
                    this._timer.Elapsed += delegate
                        {
                            this._timer.Stop();
                            Thread.CurrentThread.Name = this._invokingThreadName;
                            long ticksBefore = SharedStopWatch.ElapsedTicks;
                            this.performAction();
                            long milliSecondsToWait = this._interval;
                            if (this._strictInterval)
                            {
                                milliSecondsToWait = (long)(this._interval - TimeSpan.FromTicks(SharedStopWatch.ElapsedTicks - ticksBefore).TotalMilliseconds);
                                milliSecondsToWait = Math.Max(milliSecondsToWait, 1);
                            }

                            this._timer.Interval = milliSecondsToWait;
                            this._timer.Start();
                        };
                    this._timer.AutoReset = false;
                    this._timer.Start();
                }
                else
                {
                    this._thread = new Thread(delegate()
                        {
                            Thread.Sleep((int)this._interval);
                            while (!this._isStopped)
                            {
                                long ticksBefore = SharedStopWatch.ElapsedTicks;
                                this.performAction();
                                long milliSecondsToWait = this._interval;
                                if (this._strictInterval)
                                {
                                    milliSecondsToWait = (long)(this._interval - TimeSpan.FromTicks(SharedStopWatch.ElapsedTicks - ticksBefore).TotalMilliseconds);
                                }

                                if (milliSecondsToWait > 0)
                                {
                                    Thread.Sleep((int)milliSecondsToWait);
                                }
                            }
                        })
                        {
                            Name = this._invokingThreadName, 
                            IsBackground = true
                        };
                    this._thread.Start();
                }
            }
        }

        public void Stop()
        {
            this._isStopped = true;
            if (this._timer != null)
            {
                this._timer.Stop();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.Stop();
            if (disposing)
            {
                this._lock.EnterWriteLock();
            }

            try
            {
                this._actions.Clear();
            }
            finally
            {
                if (disposing)
                {
                    this._lock.ExitWriteLock();
                }
            }

            this._lock.Dispose();
        }

        private void performAction()
        {
            this._lock.EnterReadLock();
            try
            {
                foreach (var kvp in this._actions)
                {
                    try
                    {
                        kvp.Key(kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        #endregion Methods
    }
}
