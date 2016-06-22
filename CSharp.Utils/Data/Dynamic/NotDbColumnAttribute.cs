using System;

namespace CSharp.Utils.Data.Dynamic
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class NotDbColumnAttribute : Attribute
    {
    }
}
