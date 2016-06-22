using System;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class FlushEventArgs<T> : EventArgs
    {
        #region Constructors and Finalizers

        public FlushEventArgs(ICollection<T> items)
        {
            this.Items = items;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public ICollection<T> Items { get; private set; }

        #endregion Public Properties
    }
}
