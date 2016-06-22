using System;

namespace CSharp.Utils.Data.Sync
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotSyncColumnAttribute : Attribute
    {
    }
}
