using System;
using System.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Reflection
{
    public sealed class InjectableMethodInfo<T>
        where T : Attribute
    {
        #region Constructors and Finalizers

        public InjectableMethodInfo(MethodInfo method, string methodName, T attribute)
        {
            Guard.ArgumentNotNull(method, "method");
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(methodName, "methodName");
            Guard.ArgumentNotNull(attribute, "attribute");
            this.Method = method;
            this.MethodName = methodName;
            this.Attribute = attribute;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Attribute { get; private set; }

        public MethodInfo Method { get; private set; }

        public string MethodName { get; private set; }

        #endregion Public Properties
    }
}
