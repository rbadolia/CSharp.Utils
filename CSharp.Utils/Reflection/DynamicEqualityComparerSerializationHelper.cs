using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;

namespace CSharp.Utils.Reflection
{
    [Serializable]
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [SecurityPermission(SecurityAction.LinkDemand, 
        Flags = SecurityPermissionFlag.SerializationFormatter)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, 
       Level = AspNetHostingPermissionLevel.Minimal)]
    internal sealed class DynamicEqualityComparerSerializationHelper<T> : IObjectReference 
    {
        public object GetRealObject(StreamingContext context)
        {
            return DynamicEqualityComparer<T>.Instance;
        }
    }
}
