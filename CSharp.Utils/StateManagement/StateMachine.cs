using System;
using System.Collections.Generic;
using CSharp.Utils.Authorization;
using CSharp.Utils.Contracts;
using CSharp.Utils.EventProcessing;
using CSharp.Utils.Validation;

namespace CSharp.Utils.StateManagement
{
    public  class StateMachine : IStateMachine<string, string>
    {
        private readonly Func<string> _stateAccessor;

        private readonly StateMachineCore _core;

        private readonly IIdentity _obj;

        private readonly string _entityTypeName;

        public StateMachine(IIdentity obj, StateMachineCore core)
        {
            Guard.ArgumentNotNull(obj, "obj");
            Guard.ArgumentNotNull(core, "core");

            this._entityTypeName = obj.GetType().Name;

            this._obj = obj;
            this._core = core;
            this._stateAccessor = () => (string) this._core.StateProperty.GetValue(this._obj);
        }

        private void ChangeState(string nextState)
        {
            var oldState = this._stateAccessor();
            this._core.StateProperty.SetValue(this._obj, nextState);
            if (this._core.PublishStateTransitionEvents)
            {
                if (!nextState.Equals(oldState))
                {
                    string subject = string.Format("{0}.StateTransition.From.{1}.To.{2}", this._entityTypeName, oldState, nextState);
                    var evt = new StateTransitionEvent(this._obj.Id, this._entityTypeName, subject);
                    EventPublisher.Instance.PublishEvent(evt);
                }
            }
        }

        public AuthorizationSettings GetAuthorizationSettingsForAction(string action)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(action, "action");
            return this._core.GetAuthorizationSettingsForAction(action);
        }

        public string GetDotGraph(bool excludeActionsWithNoTransition)
        {
            var currentState = this._stateAccessor();
            return this._core.GetDotGraph(excludeActionsWithNoTransition, currentState);
        }

        public bool CanPerformAction(string action)
        {
            var currentState = this._stateAccessor();
            return this._core.CanFire(action, currentState);
        }

        public void PerformAction(string action, Action actionDelegate)
        {
            var currentState = this._stateAccessor();
            var nextState = this._core.Fire(action, currentState, actionDelegate);
            this.ChangeState(nextState);
        }

        public IEnumerable<string> GetPermittedActions()
        {
            return this._core.GetPermittedActions(this._stateAccessor());
        }

        public IReadOnlyCollection<string> AllStates
        {
            get { return this._core.AllStates; }
        }

        public IReadOnlyCollection<string> AllActions
        {
            get { return this._core.AllActions; }
        }
    }
}
