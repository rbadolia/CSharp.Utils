using System;
using System.Collections.Generic;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.EventProcessing.Contracts
{
    public interface IEvent : IIdentity
    {
        Guid CommandScopeId { get; }

        string Subject { get; }

        IList<Guid> GetRelatedEntityIds();

        DateTime OccuredOn { get; }

        string Message { get; }
    }
}
