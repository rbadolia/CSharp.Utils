using System;
using CSharp.Utils.EventProcessing.Contracts;
using CSharp.Utils.Validation;

namespace CSharp.Utils.EventProcessing
{
    public sealed class EventArg : EventArgs
    {
        public EventArg(IEvent eventObject, bool notityingInSameTransaction, bool isRemoteEvent)
        {
            Guard.ArgumentNotNull(eventObject, "eventObject");
            this.EventObject = eventObject;
            this.NotityingInSameTransaction = notityingInSameTransaction;
            this.IsRemoteEvent = isRemoteEvent;
        }

        public IEvent EventObject { get; private set; }

        public bool IsRemoteEvent { get; private set; }

        public bool NotityingInSameTransaction { get; private set; }
    }
}
