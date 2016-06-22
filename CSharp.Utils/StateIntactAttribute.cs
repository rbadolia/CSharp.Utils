using System;

namespace CSharp.Utils
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Property|AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    public sealed class StateIntactAttribute : Attribute
    {
    }
}
