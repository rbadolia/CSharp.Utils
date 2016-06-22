using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    public abstract class AbstractComponentController : IComponentController
    {
        #region Constructors and Finalizers

        protected AbstractComponentController(bool canPause)
        {
            this.State = ControllableComponentState.NotStarted;
            this.SupportsPauseAndResume = canPause;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public ControllableComponentState State { get; private set; }

        public bool SupportsPauseAndResume { get; private set; }

        #endregion Public Properties

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

        #region Methods

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
                }
            }
        }

        private void Start()
        {
            if (this.State == ControllableComponentState.NotStarted || this.State == ControllableComponentState.Stopped)
            {
                this.State = ControllableComponentState.Starting;
                this.StartProtected();
                this.State = ControllableComponentState.Idle;
            }
        }

        public void StartedDoingSomething()
        {
            this.State = ControllableComponentState.Busy;
        }

        private void Stop()
        {
            if (this.State == ControllableComponentState.Idle ||
                this.SupportsPauseAndResume && this.State == ControllableComponentState.Busy)
            {
                this.State = ControllableComponentState.Stopping;
                this.StopProtected();
                this.State = ControllableComponentState.Stopped;
            }
        }

        public void StoppedDoingSomething()
        {
            this.State = ControllableComponentState.Idle;
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
