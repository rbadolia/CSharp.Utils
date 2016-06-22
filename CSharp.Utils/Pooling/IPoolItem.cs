using System;

namespace CSharp.Utils.Pooling
{
    public interface IPoolItem<T> : IDisposable
    {
        #region Public Methods and Operators

        void SetAdaptedObjectAndCallback(T adaptedObject, ReturnAdaptedObjectToPoolDelegate<T> callback);

        #endregion Public Methods and Operators
    }
}
