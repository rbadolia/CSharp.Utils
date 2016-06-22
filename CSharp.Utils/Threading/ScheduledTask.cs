using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using CSharp.Utils.Validation;
using Timer = System.Timers.Timer;

namespace CSharp.Utils.Threading
{
    public sealed class ScheduledTask : AbstractDisposable
    {
        #region Fields

        private readonly bool _performTaskOnStart;

        private readonly object _syncLockForStart = new object();

        private readonly Timer _timer;

        private readonly Time[] _timings;

        private int _index = -1;

        private volatile bool _isStarted;

        private DateTime _nextRunOn;

        #endregion Fields

        #region Constructors and Finalizers

        public ScheduledTask(TaskScheduleSettings settings)
            : this(settings.Timings, settings.PerformTaskOnStart)
        {
        }

        public ScheduledTask(IEnumerable<Time> timings, bool performTaskOnStart)
            : this(performTaskOnStart, timings.ToArray())
        {
        }

        public ScheduledTask(bool performTaskOnStart, params Time[] timings)
        {
            Guard.ArgumentNotNull(timings, "timings");
            if (timings.Length == 0)
            {
                throw new ArgumentException("Timings should not be empty", "timings");
            }

            this._performTaskOnStart = performTaskOnStart;
            var hashSet = new HashSet<Time>();
            foreach (Time time in timings)
            {
                hashSet.Add(time);
            }

            List<Time> temp = hashSet.ToList();
            temp.Sort(TimeComparer.Instance);
            this._timings = temp.ToArray();
            this._timer = new Timer();
            this._timer.Elapsed += this.TimerElapsed;
            this._timer.Enabled = false;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler OnSchedule;

        #endregion Public Events

        #region Public Properties

        public bool IsStarted
        {
            get
            {
                return this._isStarted;
            }

            private set
            {
                this._isStarted = value;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Start()
        {
            if (!this.IsStarted)
            {
                lock (this._syncLockForStart)
                {
                    if (!this.IsStarted)
                    {
                        this.IsStarted = true;
                        if (this._performTaskOnStart)
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                                {
                                    this.raiseOnSchedule();
                                    this.startCore();
                                });
                        }
                        else
                        {
                            this.startCore();
                        }
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            GeneralHelper.DisposeIDisposable(this._timer);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._timer.Stop();
            try
            {
                Debug.WriteLine("raising event on: " + GlobalSettings.Instance.CurrentDateTime.ToString(CultureInfo.InvariantCulture));
                this.raiseOnSchedule();
            }
            finally
            {
                this._timer.Interval = this.determineNextRunOn();
                this._timer.Start();
            }
        }

        private long determineNextRunOn()
        {
            int daysToAdd = 0;
            DateTime currentDate = DateTime.Now;
            var currentTime = new Time(currentDate);
            if (this._index == this._timings.Length - 1)
            {
                this._index = 0;
                daysToAdd = 1;
            }
            else
            {
                this._index = (this._index + 1) % this._timings.Length;
                for (; this._index < this._timings.Length; this._index++)
                {
                    if (currentTime <= this._timings[this._index])
                    {
                        break;
                    }
                }

                if (this._index == this._timings.Length)
                {
                    this._index = 0;
                }
            }

            Time t = this._timings[this._index];
            this._nextRunOn = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, t.Hour, t.Minute, t.Second, t.Millisecond);
            this._nextRunOn = this._nextRunOn.AddDays(daysToAdd);
            var waitTime = (long)(this._nextRunOn - DateTime.Now).TotalMilliseconds;
            return Math.Max(waitTime, 1);
        }

        private void raiseOnSchedule()
        {
            if (this.OnSchedule != null)
            {
                this.OnSchedule(this, EventArgs.Empty);
            }
        }

        private void startCore()
        {
            this._timer.Interval = this.determineNextRunOn();
            this._timer.Enabled = true;
            this._timer.Start();
        }

        #endregion Methods
    }
}
