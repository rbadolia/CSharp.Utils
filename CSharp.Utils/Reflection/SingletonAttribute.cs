using System;

namespace CSharp.Utils.Reflection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SingletonAttribute : Attribute
    {
        #region Constructors and Finalizers

        public SingletonAttribute()
        {
            this.InstancePropertyName = "Instance";
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string InstancePropertyName { get; set; }

        #endregion Public Properties
    }
}
