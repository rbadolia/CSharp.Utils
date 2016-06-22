using System;

namespace CSharp.Utils.Data.Auditing
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SkipAuditingAttribute : Attribute
    {
        public SkipAuditingAttribute(bool skipAuditing = true)
        {
            this.Enabled = skipAuditing;
        }

        public bool Enabled { get; set; }
    }
}
