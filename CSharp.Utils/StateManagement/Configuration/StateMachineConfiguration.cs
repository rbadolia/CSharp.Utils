using System;
using System.Collections.Generic;

namespace CSharp.Utils.StateManagement.Configuration
{
    public class StateMachineConfiguration
    {
        public Type TargetType { get; set; }

        public string StatePropertyName { get; set; }

        public bool PublishStateTransitionEvents { get; set; }

        public StateMachineConfiguration()
        {
            this.Actions = new List<ActionConfiguration>();
        }

        public List<ActionConfiguration> Actions { get; set; }
    }
}
