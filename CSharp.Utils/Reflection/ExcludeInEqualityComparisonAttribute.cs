using System;

namespace CSharp.Utils.Reflection
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExcludeInEqualityComparisonAttribute : Attribute
    {
    }
}
