using System.Collections.Generic;
using CSharp.Utils.Authorization;

namespace CSharp.Utils.StateManagement.Configuration
{
    public class ActionConfiguration
    {
        public ActionConfiguration()
        {
            this.StateTransitions = new List<StateTransitionConfiguration>();
        }

        public string Action { get; set; }

        public AuthorizationSettings AuthorizationSettings { get; set; }

        public List<StateTransitionConfiguration> StateTransitions { get; set; }
    }
}
