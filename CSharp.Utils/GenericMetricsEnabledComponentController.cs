using CSharp.Utils.Validation;

namespace CSharp.Utils
{
    public class GenericMetricsEnabledComponentController : AbstractMetricsEnabledComponentController
    {
        #region Fields

        private readonly ControllerActionDelegate _pauseCallback;

        private readonly ControllerActionDelegate _resumeCallback;

        private readonly ControllerActionDelegate _startCallback;

        private readonly ControllerActionDelegate _stopCallback;

        #endregion Fields

        #region Constructors and Finalizers

        public GenericMetricsEnabledComponentController(ControllerActionDelegate startCallback, ControllerActionDelegate stopCallback)
            : base(false)
        {
            Guard.ArgumentNotNull(startCallback, "startCallback");
            Guard.ArgumentNotNull(stopCallback, "stopCallback");
            this._startCallback = startCallback;
            this._stopCallback = stopCallback;
        }

        public GenericMetricsEnabledComponentController(ControllerActionDelegate startCallback, ControllerActionDelegate stopCallback, ControllerActionDelegate pauseCallback, ControllerActionDelegate resumeCallback)
            : base(true)
        {
            Guard.ArgumentNotNull(startCallback, "startCallback");
            Guard.ArgumentNotNull(stopCallback, "stopCallback");
            Guard.ArgumentNotNull(resumeCallback, "pauseCallback");
            Guard.ArgumentNotNull(resumeCallback, "resumeCallback");
            this._startCallback = startCallback;
            this._stopCallback = stopCallback;
            this._pauseCallback = pauseCallback;
            this._resumeCallback = resumeCallback;
        }

        #endregion Constructors and Finalizers

        #region Methods

        protected override void PauseProtected()
        {
            this._pauseCallback();
        }

        protected override void ResumeProtected()
        {
            this._resumeCallback();
        }

        protected override void StartProtected()
        {
            this._startCallback();
        }

        protected override void StopProtected()
        {
            this._stopCallback();
        }

        #endregion Methods
    }
}
