using System;

namespace CSharp.Utils.StateManagement.Configuration
{
    [Serializable]
    public class StateTransitionConfiguration
    {
        public string FromState { get; set; }

        public string ToState { get; set; }
    }
}
