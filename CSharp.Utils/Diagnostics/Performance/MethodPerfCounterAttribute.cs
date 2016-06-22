using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class MethodPerfCounterAttribute : Attribute
    {
    }
}
