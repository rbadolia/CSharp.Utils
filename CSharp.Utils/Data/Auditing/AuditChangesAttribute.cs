using System;

namespace CSharp.Utils.Data.Auditing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuditChangesAttribute : Attribute
    {
        public AuditChangesAttribute(bool auditChanges = true)
        {
            this.Enabled = auditChanges;
        }

        public bool Enabled { get; set; }
    }
}
