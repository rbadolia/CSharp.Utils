using System;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    [Serializable]
    internal sealed class MethodCallInformation : AbstractMethodInformation
    {
        #region Public Properties

        public string MethodName { get; set; }

        #endregion Public Properties
    }
}
