using System;

namespace CSharp.Utils.Data.Sync
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnMappingAttribute : Attribute
    {
        #region Public Properties

        public string TargetColumnName { get; set; }

        #endregion Public Properties
    }
}
