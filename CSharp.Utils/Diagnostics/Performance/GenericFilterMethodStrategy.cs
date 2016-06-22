using System;
using System.Collections.Generic;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class GenericFilterMethodStrategy<T> : IFilterMethodsStrategy<T>
        where T : Attribute
    {
        #region Static Fields

        private static readonly GenericFilterMethodStrategy<T> InstanceObject = new GenericFilterMethodStrategy<T>();

        #endregion Static Fields

        #region Constructors and Finalizers

        private GenericFilterMethodStrategy()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static GenericFilterMethodStrategy<T> Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public Type DoNotInjectAttributeType { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public IEnumerable<InjectableMethodInfo<T>> FilterMethods(Assembly assembly)
        {
            return MethodUtil.GetInjectableMethods<T>(assembly, this.DoNotInjectAttributeType);
        }

        #endregion Public Methods and Operators
    }
}
