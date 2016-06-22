using System;
using System.Collections.Generic;
using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    public sealed class CompositeControllableComponent : AbstractInitializableAndDisposable, ICompositeControllable
    {
        #region Fields

        private readonly GenericComponentController _controller;

        #endregion Fields

        #region Constructors and Finalizers

        public CompositeControllableComponent(IEnumerable<IControllable> components)
            : this()
        {
            foreach (IControllable component in components)
            {
                this.Controllables.Add(component);
            }
        }

        public CompositeControllableComponent()
        {
            this.Controllables = new List<IControllable>();
            this._controller = new GenericComponentController(this.Start, this.Stop, this.Pause, this.Resume);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public IList<IControllable> Controllables { get; set; }

        public IComponentController Controller
        {
            get
            {
                return this._controller;
            }
        }

        public string Name { get; set; }

        #endregion Public Properties

        #region Methods

        protected override void Dispose(bool disposing)
        {
            foreach (IControllable controllable in this.Controllables)
            {
                var disposable = controllable as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        protected override void InitializeProtected()
        {
            foreach (IControllable controllable in this.Controllables)
            {
                var initializable = controllable as IInitializable;
                if (initializable != null && !initializable.IsInitialized)
                {
                    initializable.Initialize();
                }
            }
        }

        private void Pause()
        {
            foreach (IControllable controllable in this.Controllables)
            {
                if (controllable.Controller != null)
                {
                    controllable.Controller.PerformControllableAction(ControllableAction.Pause);
                }
            }
        }

        private void Resume()
        {
            foreach (IControllable controllable in this.Controllables)
            {
                if (controllable.Controller != null)
                {
                    controllable.Controller.PerformControllableAction(ControllableAction.Resume);
                }
            }
        }

        private void Start()
        {
            foreach (IControllable controllable in this.Controllables)
            {
                if (controllable.Controller != null)
                {
                    controllable.Controller.PerformControllableAction(ControllableAction.Start);
                }
            }
        }

        private void Stop()
        {
            foreach (IControllable controllable in this.Controllables)
            {
                if (controllable.Controller != null)
                {
                    controllable.Controller.PerformControllableAction(ControllableAction.Stop);
                }
            }
        }

        #endregion Methods

        public bool IsRunning
        {
            get { return true; }
        }
    }
}
