using System;
using System.Xml.Serialization;
using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    [Serializable]
    [XmlRoot("component")]
    public abstract class AbstractControllableComponent : AbstractComponent, IControllable
    {
        #region Constructors

        private IComponentController _controller;

        protected AbstractControllableComponent()
        {
            _controller = new GenericMetricsEnabledComponentController(this.StartProtected, this.StopProtected, this.PauseProtected, this.ResumeProtected);
        }

        #endregion Constructors

        public bool IsRunning
        {
            get
            {
                return this.Controller.State == ControllableComponentState.Busy ||
                       this.Controller.State == ControllableComponentState.Idle;
            }
        }

        #region Methods

        protected virtual void PauseProtected()
        {
        }

        protected virtual void ResumeProtected()
        {
        }

        protected virtual void StartProtected()
        {
        }

        protected virtual void StopProtected()
        {
        }

        #endregion Methods

        public IComponentController Controller
        {
            get { return this._controller; }
        }
    }
}
