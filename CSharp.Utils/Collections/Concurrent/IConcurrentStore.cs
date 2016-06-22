using System;

namespace CSharp.Utils.Collections.Concurrent
{
    public interface IConcurrentStore<T> : IDisposable
    {
        #region Public Events

        event EventHandler<StoreFlushEventArgs<T>> OnStoreFlush;

        #endregion Public Events

        #region Public Properties

        long MaxWaitTime { get; }

        #endregion Public Properties

        #region Public Methods and Operators

        bool Add(T item);

        void ForceFlush();

        #endregion Public Methods and Operators
    }
}
