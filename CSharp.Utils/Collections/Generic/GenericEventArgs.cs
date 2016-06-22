using System;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class GenericEventArgs<T> : EventArgs
    {
        #region Constructors and Finalizers

        public GenericEventArgs(T value)
        {
            this.Value = value;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Value { get; private set; }

        #endregion Public Properties
    }
}
