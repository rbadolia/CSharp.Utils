using System;
using System.Reflection;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    [Serializable]
    public abstract class AbstractMethodInformation
    {
        #region Properties

        internal string[] ParameterTypes { get; set; }

        #endregion Properties

        #region Methods

        internal static void PopulateMethodBaseInformation(MethodBase method, AbstractMethodInformation info)
        {
            ParameterInfo[] parameters = method.GetParameters();
            info.ParameterTypes = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                info.ParameterTypes[i] = parameters[i].ParameterType.AssemblyQualifiedName;
            }
        }

        #endregion Methods
    }
}
