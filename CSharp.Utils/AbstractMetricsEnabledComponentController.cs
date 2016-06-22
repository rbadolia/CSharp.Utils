using System;
using System.Threading;
using CSharp.Utils.Contracts;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Diagnostics.Performance;

namespace CSharp.Utils
{
    public abstract class AbstractMetricsEnabledComponentController : IMetricsEnabledComponentController
    {
        #region Fields

        private long busyStartedAt;

        private long idleStartedAt;

        private long numberOfThreadsBeingServed;

        private long pauseStartedAt;

        private long stopStartedAt;

        private long totalTicksInBusy;

        private long totalTicksInIdle;

        private long totalTicksInPaused;

        private long totalTicksInStopped;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractMetricsEnabledComponentController(bool canPause)
        {
            this.State = ControllableComponentState.NotStarted;
            this.SupportsPauseAndResume = canPause;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        [PerfCounter]
        public double BusyPercentage
        {
            get
            {
                long ticks = this.TimeSpentInBusy.Ticks;
                long totalTicks = this.TimeSpentInIdle.Ticks + ticks;
                if (totalTicks == 0)
                {
                    return 0;
                }

                return 100*ticks/(double) totalTicks;
            }
        }

        [PerfCounter]
        public double IdlePercentage
        {
            get
            {
                long ticks = this.TimeSpentInIdle.Ticks;
                long totalTicks = this.TimeSpentInBusy.Ticks + ticks;
                if (totalTicks == 0)
                {
                    return 0;
                }

                return 100*ticks/(double) totalTicks;
            }
        }

        [PerfCounter]
        public long NumberOfThreadsBeingServed
        {
            get { return Interlocked.Read(ref this.numberOfThreadsBeingServed); }
        }

        public ControllableComponentState State { get; private set; }

        public bool SupportsPauseAndResume { get; private set; }

        [PerfCounter]
        public TimeSpan TimeSpentInBusy
        {
            get
            {
                long ticks = Interlocked.Read(ref this.totalTicksInBusy);
                if (this.State == ControllableComponentState.Busy)
                {
                    ticks += SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.busyStartedAt);
                }

                return TimeSpan.FromTicks(ticks);
            }
        }

        [PerfCounter]
        public TimeSpan TimeSpentInIdle
        {
            get
            {
                long ticks = Interlocked.Read(ref this.totalTicksInIdle);
                if (this.State == ControllableComponentState.Idle)
                {
                    ticks += SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.idleStartedAt);
                }

                return TimeSpan.FromTicks(ticks);
            }
        }

        [PerfCounter]
        public TimeSpan TimeSpentInPaused
        {
            get
            {
                long ticks = Interlocked.Read(ref this.totalTicksInPaused);
                if (this.State == ControllableComponentState.Paused)
                {
                    ticks += SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.pauseStartedAt);
                }

                return TimeSpan.FromTicks(ticks);
            }
        }

        [PerfCounter]
        public TimeSpan TimeSpentInStopped
        {
            get
            {
                long ticks = Interlocked.Read(ref this.totalTicksInStopped);
                if (this.State == ControllableComponentState.Stopped)
                {
                    ticks += SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.stopStartedAt);
                }

                return TimeSpan.FromTicks(ticks);
            }
        }

        #endregion Public Properties

        #region Methods

        public void PerformControllableAction(ControllableAction action)
        {
            switch (action)
            {
                case ControllableAction.Pause:
                    this.Pause();
                    break;
                case ControllableAction.Resume:
                    this.Resume();
                    break;
                case ControllableAction.Start:
                    this.Start();
                    break;
                case ControllableAction.Stop:
                    this.Stop();
                    break;
            }
        }

        private void Pause()
        {
            if (this.SupportsPauseAndResume)
            {
                if (this.SupportsPauseAndResume && this.State == ControllableComponentState.Idle ||
                    this.SupportsPauseAndResume && this.State == ControllableComponentState.Busy)
                {
                    this.State = ControllableComponentState.Pausing;
                    this.PauseProtected();
                    this.State = ControllableComponentState.Paused;
                    this.pauseStartedAt = SharedStopWatch.ElapsedTicks;
                }
            }
        }

        private void Resume()
        {
            if (this.SupportsPauseAndResume)
            {
                if (this.State == ControllableComponentState.Paused)
                {
                    this.State = ControllableComponentState.Resuming;
                    this.ResumeProtected();
                    this.State = ControllableComponentState.Idle;
                    Interlocked.Add(ref this.totalTicksInPaused, 
                        SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.pauseStartedAt));
                    this.idleStartedAt = SharedStopWatch.ElapsedTicks;
                }
            }
        }

        private void Start()
        {
            if (this.State == ControllableComponentState.NotStarted || this.State == ControllableComponentState.Stopped)
            {
                if (this.State == ControllableComponentState.Stopped)
                {
                    Interlocked.Add(ref this.totalTicksInStopped, 
                        SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.stopStartedAt));
                }

                this.State = ControllableComponentState.Starting;
                this.StartProtected();
                this.State = ControllableComponentState.Idle;
                this.idleStartedAt = SharedStopWatch.ElapsedTicks;
            }
        }

        public void StartedDoingSomething()
        {
            if (Interlocked.Increment(ref this.numberOfThreadsBeingServed) == 1)
            {
                Interlocked.Add(ref this.totalTicksInIdle, 
                    SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.idleStartedAt));
                this.busyStartedAt = SharedStopWatch.ElapsedTicks;
                this.State = ControllableComponentState.Busy;
            }
        }

        private void Stop()
        {
            if (this.State == ControllableComponentState.Idle ||
                this.SupportsPauseAndResume && this.State == ControllableComponentState.Busy)
            {
                if (this.State == ControllableComponentState.Idle)
                {
                    Interlocked.Add(ref this.totalTicksInIdle, 
                        SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.idleStartedAt));
                }
                else
                {
                    Interlocked.Add(ref this.totalTicksInBusy, 
                        SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.busyStartedAt));
                }

                this.State = ControllableComponentState.Stopping;
                this.StopProtected();
                this.State = ControllableComponentState.Stopped;
                this.stopStartedAt = SharedStopWatch.ElapsedTicks;
            }
        }

        public void StoppedDoingSomething()
        {
            if (Interlocked.Decrement(ref this.numberOfThreadsBeingServed) == 0)
            {
                Interlocked.Add(ref this.totalTicksInBusy, 
                    SharedStopWatch.ElapsedTicks - Interlocked.Read(ref this.busyStartedAt));
                this.State = ControllableComponentState.Idle;
                this.idleStartedAt = SharedStopWatch.ElapsedTicks;
            }
        }

        protected virtual void PauseProtected()
        {
        }

        protected virtual void ResumeProtected()
        {
        }

        protected abstract void StartProtected();

        protected abstract void StopProtected();

        #endregion Methods
    }
}
