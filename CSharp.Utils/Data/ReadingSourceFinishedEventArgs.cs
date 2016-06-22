using System;

namespace CSharp.Utils.Data
{
    public sealed class ReadingSourceFinishedEventArgs : EventArgs
    {
        #region Constructors and Finalizers

        public ReadingSourceFinishedEventArgs(int sourceNumber)
        {
            this.SourceNumber = sourceNumber;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int SourceNumber { get; private set; }

        #endregion Public Properties
    }
}
