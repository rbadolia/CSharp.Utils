using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using CSharp.Utils.Configuration;
using CSharp.Utils.Contracts;
using CSharp.Utils.Reflection;
using CSharp.Utils.StateManagement.Configuration;
using CSharp.Utils.Validation;

namespace CSharp.Utils.StateManagement
{
    public class StateMachineFactory : IConfigurable
    {
        public static readonly string InitialStatus = "NULL";

        public static readonly string CreateNewActionName = "CreateNew";

        public static readonly string DisposeActionName = "Dispose";

        private static StateMachineFactory _instance = new StateMachineFactory();

        private StateMachineFactory()
        {

        }

        public static StateMachineFactory Instance
        {
            get { return _instance; }
        }

        public void Configure(XmlNode configurationNode, IObjectInstantiator instantiator = null)
        {
            var configurations = new List<StateMachineConfiguration>();

            ComponentBuilder.PopulateList(configurations, configurationNode.ChildNodes[0], instantiator);
            foreach (var configuration in configurations)
            {
               this.Configure(configuration);
            }
        }

        public void Configure(StateMachineConfiguration configuration)
        {
            Guard.ArgumentNotNull(configuration, "configuration");
            var core = BuildStatemachineCore(configuration);
            this._stateMachineCores.Add(configuration.TargetType, core);
        }

        private Dictionary<Type, StateMachineCore> _stateMachineCores =
            new Dictionary<Type, StateMachineCore>();

        private static StateMachineCore BuildStatemachineCore(StateMachineConfiguration configuration)
        {
            PropertyInfo stateProperty = GetStateProperty(configuration.TargetType, configuration.StatePropertyName);
            var stateMachineCore = new StateMachineCore(stateProperty, configuration);
            stateMachineCore.Initialize();
            return stateMachineCore;
        }

        public StateMachine BuildStateMachine(IIdentity obj)
        {
            Guard.ArgumentNotNull(obj, "obj");
            StateMachineCore stateMachineCore;
            if (!this._stateMachineCores.TryGetValue(obj.GetType(), out stateMachineCore))
            {
                throw new Exception();
            }

            var stateMachine = new StateMachine(obj, stateMachineCore);

            return stateMachine;
        }

        public string GetDotGraph<T>(bool excludeActionsWithNoTransition)
        {
            return GetDotGraph(typeof(T), excludeActionsWithNoTransition);
        }

        public string GetDotGraph(Type type, bool excludeActionsWithNoTransition)
        {
            Guard.ArgumentNotNull(type, "type");
            StateMachineCore stateMachineCore;
            if (!this._stateMachineCores.TryGetValue(type, out stateMachineCore))
            {
                return null;
            }

            return stateMachineCore.GetDotGraph(excludeActionsWithNoTransition, null);
        }

        private static PropertyInfo GetStateProperty(Type type, string propertyName)
        {
            const string PropertyErrorMessage = "Invalid State property";
            var property = type.GetProperty(propertyName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                throw new StateMachineException("Invalid State property");
            }

            if (property.PropertyType != typeof(string))
            {
                throw new StateMachineException(PropertyErrorMessage);
            }

            if (!property.CanRead || !property.CanWrite)
            {
                throw new StateMachineException(PropertyErrorMessage);
            }

            return property;
        }
    }
}
