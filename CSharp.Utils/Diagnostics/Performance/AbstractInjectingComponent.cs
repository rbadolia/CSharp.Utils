using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public abstract class AbstractInjectingComponent<T>
        where T : Attribute
    {
        #region Fields

        private bool isEnabled;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractInjectingComponent(IFilterMethodsStrategy<T> filterMethodsStrategy)
        {
            this.FilterMethodsStrategy = filterMethodsStrategy;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool Enabled
        {
            get
            {
                return this.isEnabled;
            }

            set
            {
                if (value != this.isEnabled)
                {
                    if (value)
                    {
                        AppDomainHelper.AssemblyLoad += this.AppDomainHelper_AssemblyLoad;
                    }
                    else
                    {
                        AppDomainHelper.AssemblyLoad -= this.AppDomainHelper_AssemblyLoad;
                    }

                    this.isEnabled = value;
                }
            }
        }

        public IFilterMethodsStrategy<T> FilterMethodsStrategy { get; set; }

        #endregion Public Properties

        #region Methods

        protected abstract DynamicMethod BuildDynamicMethod(InjectableMethodInfo<T> methodInfo);

        private void AppDomainHelper_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                this.replaceMethods(args.LoadedAssembly);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void replaceMethod(InjectableMethodInfo<T> methodInfo)
        {
            RuntimeHelpers.PrepareMethod(methodInfo.Method.MethodHandle);
            try
            {
                DynamicMethod dynamicMethod = this.BuildDynamicMethod(methodInfo);
                MethodUtil.ReplaceMethod(dynamicMethod, methodInfo.Method);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void replaceMethods(Assembly assembly)
        {
            IFilterMethodsStrategy<T> strategy = this.FilterMethodsStrategy;
            if (strategy != null)
            {
                IEnumerable<InjectableMethodInfo<T>> methods = strategy.FilterMethods(assembly);
                foreach (var methodInfo in methods)
                {
                    this.replaceMethod(methodInfo);
                }
            }
        }

        #endregion Methods
    }
}
