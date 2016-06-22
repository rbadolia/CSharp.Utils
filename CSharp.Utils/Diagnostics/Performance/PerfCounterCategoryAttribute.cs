using System;

namespace CSharp.Utils.Diagnostics.Performance
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class PerfCounterCategoryAttribute : Attribute
    {
        #region Constructors and Finalizers

        public PerfCounterCategoryAttribute(string categoryName, string categoryHelp)
        {
            this.CategoryName = categoryName;
            this.CategoryHelp = categoryHelp;
        }

        public PerfCounterCategoryAttribute(string categoryName)
            : this(categoryName, categoryName)
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryHelp { get; private set; }

        public string CategoryName { get; private set; }

        #endregion Public Properties
    }
}
