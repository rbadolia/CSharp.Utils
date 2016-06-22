using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class DoNotRecordExecutionFlowAttribute : Attribute
    {
    }
}
