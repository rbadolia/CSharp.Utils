using System;
using System.Runtime.Serialization;
using CSharp.Utils.Data;
using CSharp.Utils.EventProcessing.Abstractions;
using CSharp.Utils.Validation;

namespace CSharp.Utils.EventProcessing
{
    [Serializable]
    [DataContract]
    public class CudEvent : AbstractEntityEvent
    {
        public CudEvent(Guid entityId, string entityTypeName, CudOperationType operationType, string subject)
            : base(entityId, entityTypeName, subject)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(entityTypeName, "entityTypeName");
            this.OperationType = operationType;
            this.Message = string.Format("{0} {1} {2}d", entityTypeName, entityId.ToString(), operationType.ToString());
        }

        [DataMember]
        public CudOperationType OperationType { get; private set; }
    }
}
