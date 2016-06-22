using System;
using CSharp.Utils.Diagnostics.Performance.Dtos;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class CountersCapturedEventArgs : EventArgs
    {
        #region Constructors and Finalizers

        public CountersCapturedEventArgs(CounterUpdateInfo info)
        {
            this.Info = info;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public CounterUpdateInfo Info { get; private set; }

        #endregion Public Properties
    }
}
