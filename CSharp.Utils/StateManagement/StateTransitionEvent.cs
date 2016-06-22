using System;
using System.Runtime.Serialization;
using CSharp.Utils.EventProcessing.Abstractions;

namespace CSharp.Utils.StateManagement
{
    [Serializable]
    [DataContract]
    public sealed class StateTransitionEvent : AbstractEntityEvent
    {
        public StateTransitionEvent(Guid entityId, string entityTypeName, string subject) : base(entityId, entityTypeName, subject)
        {

        }

        public StateTransitionEvent(Guid entityId, string entityTypeName, string subject, string message)
            : base(entityId, entityTypeName, subject, message)
        {
        }
    }
}
