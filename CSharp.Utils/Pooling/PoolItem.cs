using System;

namespace CSharp.Utils.Pooling
{
    public class PoolItem<T> : AbstractDisposable, IPoolItem<T>
    {
        #region Fields

        private T adaptedObject;

        private ReturnAdaptedObjectToPoolDelegate<T> _callback;

        #endregion Fields

        #region Properties

        protected T AdaptedObjectProtected
        {
            get
            {
                if (!this.IsDisposed)
                {
                    return this.adaptedObject;
                }

                throw new ObjectDisposedException("this", "The object is already disposed and the AdaptedObjectProtected is returned to the Pool");
            }
        }

        #endregion Properties

        #region Explicit Interface Methods

        void IPoolItem<T>.SetAdaptedObjectAndCallback(T adaptedObject, ReturnAdaptedObjectToPoolDelegate<T> callback)
        {
            this.adaptedObject = adaptedObject;
            this._callback = callback;
        }

        #endregion Explicit Interface Methods

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this._callback(this, this.adaptedObject);
        }

        #endregion Methods
    }
}
