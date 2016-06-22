using System;

namespace CSharp.Utils.Pooling
{
    public class GenericPool<T> : AbstractPool<T, GenericPoolItem<T>>
        where T : new()
    {
        #region Fields

        private readonly IAdaptedObjectSelectionStrategy<T> _strategy;

        #endregion Fields

        #region Constructors and Finalizers

        public GenericPool(IAdaptedObjectSelectionStrategy<T> strategy = null)
        {
            this._strategy = strategy;
        }

        public GenericPool(int maxPoolSize, int objectMaxIdleTimeInMilliseconds, bool canObjectBeShared, IAdaptedObjectSelectionStrategy<T> strategy = null)
            : base(maxPoolSize, objectMaxIdleTimeInMilliseconds, canObjectBeShared)
        {
            this._strategy = strategy;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IAsyncResult BeginGetObject()
        {
            return this.beginGetObject(null);
        }

        public IAsyncResult BeginTryGetObject(int millisecondsToWait)
        {
            return this.beginTryGetObject(null, millisecondsToWait);
        }

        public GenericPoolItem<T> EndGetObject(IAsyncResult result)
        {
            return this.endGetObject(result);
        }

        public bool EndTryGetObject(IAsyncResult result, out GenericPoolItem<T> obj)
        {
            return this.endTryGetObject(result, out obj);
        }

        public GenericPoolItem<T> GetObject()
        {
            return this.getObject(null);
        }

        public bool TryGetObject(out GenericPoolItem<T> obj, int millisecondsToWait)
        {
            return this.tryGetObject(out obj, null, millisecondsToWait);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override bool canSelectThisAdaptedObject(T adaptedObject, bool isNewlyCreatedAdaptedObject, object tag)
        {
            if (this._strategy != null)
            {
                return true;
            }

            return base.canSelectThisAdaptedObject(adaptedObject, isNewlyCreatedAdaptedObject, tag);
        }

        protected override T getAdaptedObject()
        {
            return new T();
        }

        #endregion Methods
    }
}
