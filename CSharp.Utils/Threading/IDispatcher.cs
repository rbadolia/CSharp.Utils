using System;

namespace CSharp.Utils.Threading
{
    public interface IDispatcher
    {
        #region Methods

        void Dispatch(Action action);

        #endregion Methods
    }
}
