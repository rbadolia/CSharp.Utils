using System;

namespace CSharp.Utils
{
    [Serializable]
    public sealed class ObjectAlreadyInitializedException : InvalidOperationException
    {
        #region Constructors and Finalizers

        public ObjectAlreadyInitializedException()
        {
        }

        public ObjectAlreadyInitializedException(string message)
            : base(message)
        {
        }

        #endregion Constructors and Finalizers
    }
}
