using System;

namespace CSharp.Utils.Collections.Generic
{
    public class SizedQueueOnFullEventArgs<T> : EventArgs
    {
        #region Constructors and Finalizers

        public SizedQueueOnFullEventArgs(T item)
        {
            this.Item = item;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Item { get; private set; }

        #endregion Public Properties
    }
}
