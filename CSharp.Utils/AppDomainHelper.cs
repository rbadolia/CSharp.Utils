using System;
using System.Diagnostics;

namespace CSharp.Utils
{
    public static class AppDomainHelper
    {
        #region Constructors and Finalizers

        static AppDomainHelper()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public static event EventHandler<AssemblyLoadEventArgs> AssemblyLoad;

        #endregion Public Events

        #region Methods

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Delegate[] invocationList = AssemblyLoad.GetInvocationList();
            foreach (Delegate t in invocationList)
            {
                try
                {
                    t.DynamicInvoke(sender, args);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        #endregion Methods
    }
}
