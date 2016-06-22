using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharp.Utils.Authorization;
using CSharp.Utils.StateManagement.Configuration;
using CSharp.Utils.Validation;

namespace CSharp.Utils.StateManagement
{
    public class StateMachineCore : AbstractInitializable
    {
        private Dictionary<string, KeyValuePair<ActionConfiguration, Dictionary<string, string>>> _dictionary = new Dictionary<string, KeyValuePair<ActionConfiguration, Dictionary<string, string>>>();

        public IReadOnlyCollection<string> AllStates { get; private set; }

        public IReadOnlyCollection<string> AllActions { get; private set; }

        private readonly StateMachineConfiguration _configuration;

        public StateMachineCore(PropertyInfo stateProperty, StateMachineConfiguration configuration)
        {
            Guard.ArgumentNotNull(stateProperty, "stateProperty");
            Guard.ArgumentNotNull(configuration, "configuration");
            this.StateProperty = stateProperty;
            this._configuration = configuration;
        }

        public bool PublishStateTransitionEvents { get; private set; }

        public PropertyInfo StateProperty { get; private set; }

        public bool CanFire(string action, string currentState)
        {
            Guard.ArgumentNotNull(action, "action");
            Guard.ArgumentNotNull(currentState, "currentState");
            KeyValuePair<ActionConfiguration, Dictionary<string, string>> kvp;
            if (!this._dictionary.TryGetValue(action, out kvp))
            {
                return false;
            }

            return kvp.Value.ContainsKey(currentState);
        }

        public string Fire(string action, string currentState, Action actionDelegate)
        {
            Guard.ArgumentNotNull(action, "action");
            Guard.ArgumentNotNull(currentState, "currentState");
            Guard.ArgumentNotNull(actionDelegate, "actionDelegate");

            KeyValuePair<ActionConfiguration, Dictionary<string, string>> kvp;
            if (!this._dictionary.TryGetValue(action, out kvp))
            {
                throw new StateMachineException("This action is not allowed");
            }

            string nextState;
            if (!kvp.Value.TryGetValue(currentState, out nextState))
            {
                throw new StateMachineException("This action is not allowed");
            }

            actionDelegate();

            return nextState;
        }

        public IEnumerable<string> GetPermittedActions(string currentState)
        {
            var list = new List<string>();
            Guard.ArgumentNotNull(currentState, "currentState");
            foreach (var kvp in this._dictionary)
            {
                if (kvp.Value.Value.ContainsKey(currentState))
                {
                    list.Add(kvp.Key);
                }
            }

            return list;
        }

        protected override void InitializeProtected()
        {
            var allStates = new HashSet<string>();
            foreach (var actionConfiguration in this._configuration.Actions)
            {
                var innerDictionary = new Dictionary<string, string>();
                foreach (var stateTransition in actionConfiguration.StateTransitions)
                {
                    innerDictionary.Add(stateTransition.FromState, stateTransition.ToState);
                    allStates.Add(stateTransition.FromState);
                    allStates.Add(stateTransition.ToState);
                }

                this._dictionary.Add(actionConfiguration.Action, 
                    new KeyValuePair<ActionConfiguration, Dictionary<string, string>>(actionConfiguration, 
                        innerDictionary));
            }

            this.AllActions = this._dictionary.Select(kvp => kvp.Key).ToList().AsReadOnly();
            this.AllStates = allStates.ToList().AsReadOnly();
            this.PublishStateTransitionEvents = this._configuration.PublishStateTransitionEvents;
        }

        public AuthorizationSettings GetAuthorizationSettingsForAction(string action)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(action, "action");
            KeyValuePair<ActionConfiguration, Dictionary<string, string>> kvp;
            if (!this._dictionary.TryGetValue(action, out kvp))
            {
                return null;
            }

            return kvp.Key.AuthorizationSettings;
        }

        public string GetDotGraph(bool excludeActionsWithNoTransition, string currentState = null)
        {
            return StateMachineHelper.GetDotGraph(this._dictionary, excludeActionsWithNoTransition, currentState);

        }

    }
}
