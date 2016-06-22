using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CSharp.Utils.EventProcessing.Contracts;
using CSharp.Utils.Validation;
using CSharp.Utils.Web;

namespace CSharp.Utils.EventProcessing.Abstractions
{
    [Serializable]
    [DataContract]
    public abstract class AbstractEvent : AbstractIdentity, IEvent
    {
        protected AbstractEvent(string subject)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(subject, "subject");
            this.Subject = subject;
            this.OccuredOn = GlobalSettings.Instance.CurrentDateTime;
            var commandScope = CommandScope.Current;
            if (commandScope != null)
            {
                this.CommandScopeId = commandScope.Id;
            }
        }

        protected AbstractEvent(string subject, string message) : this(subject)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(message, "message");
            this.Message = message;
        }

        [DataMember]
        public virtual Guid CommandScopeId { get; protected set; }

        [DataMember]
        public virtual string Subject { get; protected set; }

        [DataMember]
        public DateTime OccuredOn { get; private set; }

        [DataMember]
        public virtual string Message { get; protected set; }

        public virtual IList<Guid> GetRelatedEntityIds()
        {
            return new List<Guid>();
        }

        public override string ToString()
        {
            return JsonSerializationHelper.Serialize(this);
        }
    }
}
