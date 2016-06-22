using System;

namespace CSharp.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CommandAttribute : Attribute
    {
    }
}
