using System;
using System.Collections.Generic;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class PropertyPerfCounterFilterMethodsStrategy : IFilterMethodsStrategy<PerfCounterCategoryAttribute>
    {
        #region Static Fields

        private static readonly PropertyPerfCounterFilterMethodsStrategy InstanceObject = new PropertyPerfCounterFilterMethodsStrategy();

        #endregion Static Fields

        #region Constructors and Finalizers

        private PropertyPerfCounterFilterMethodsStrategy()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static PropertyPerfCounterFilterMethodsStrategy Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public IEnumerable<InjectableMethodInfo<PerfCounterCategoryAttribute>> FilterMethods(Assembly assembly)
        {
            if (MethodUtil.IsInjectableAssembly<PerfCounterCategoryAttribute>(assembly))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        {
                            var attribute = type.GetCustomAttribute<PerfCounterCategoryAttribute>();
                            if (attribute != null)
                            {
                                MethodInfo methodInfo = type.GetMethod("Initialize");
                                if (methodInfo != null)
                                {
                                    yield return new InjectableMethodInfo<PerfCounterCategoryAttribute>(methodInfo, "Initialize", attribute);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
