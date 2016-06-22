using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.ComponentModel
{
    public class NotifyPropertyChangeInjector
    {
        private static readonly NotifyPropertyChangeInjector InstanceObject = new NotifyPropertyChangeInjector();

        private bool _isEnabled;

        private NotifyPropertyChangeInjector()
        {
            this.FilterTypesStrategy = new AttributeBasedFilterTypesStrategy(typeof(InjectNotifyPropertyChangeAttribute));
        }

        public IFilterTypesStrategy FilterTypesStrategy
        {
            get;
            set;
        }

        public static NotifyPropertyChangeInjector Instance
        {
            get { return InstanceObject; }
        }

        public bool Enabled
        {
            get { return _isEnabled; }
            set
            {
                if (value != _isEnabled)
                {
                    if (value)
                    {
                        AppDomainHelper.AssemblyLoad += CurrentDomain_AssemblyLoad;
                    }
                    else
                    {
                        AppDomainHelper.AssemblyLoad -= CurrentDomain_AssemblyLoad;
                    }

                    _isEnabled = value;
                }
            }
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                IEnumerable<Type> types = this.FilterTypesStrategy.FilterTypes(args.LoadedAssembly);
                foreach (var type in types)
                {
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(type) || typeof(INotifyPropertyChanging).IsAssignableFrom(type))
                    {
                        try
                        {
                            MethodInfo injectMethod = typeof(DynamicNotifyPropertyChangeInjectionHelper<>).MakeGenericType(new[] { type }).GetMethod("Inject", BindingFlags.Public | BindingFlags.Static);
                            injectMethod.Invoke(null, null);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
