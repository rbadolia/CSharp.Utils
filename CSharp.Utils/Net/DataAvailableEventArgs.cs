using System;

namespace CSharp.Utils.Net
{
    public class DataAvailableEventArgs : EventArgs
    {
        #region Fields

        private readonly string data;

        #endregion Fields

        #region Constructors and Finalizers

        public DataAvailableEventArgs(string output)
        {
            this.data = output;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string Data
        {
            get
            {
                return this.data;
            }
        }

        #endregion Public Properties
    }
}
