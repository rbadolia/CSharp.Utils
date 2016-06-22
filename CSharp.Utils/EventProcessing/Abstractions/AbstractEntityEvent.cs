using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CSharp.Utils.EventProcessing.Abstractions
{
    [Serializable]
    [DataContract]
    public abstract class AbstractEntityEvent : AbstractEvent
    {
        protected AbstractEntityEvent(Guid entityId, string entityTypeName, string subject)
            : base(subject)
        {
            this.EntityId = entityId;
        }

        protected AbstractEntityEvent(Guid entityId, string entityTypeName, string subject, string message)
            : base(subject, message)
        {
            this.EntityId = entityId;
        }

        [DataMember]
        public Guid EntityId { get; private set; }

        [DataMember]
        public string EntityTypeName { get; private set; }

        public override IList<Guid> GetRelatedEntityIds()
        {
            var list = base.GetRelatedEntityIds();
            list.Add(this.EntityId);
            return list;
        }
    }
}
