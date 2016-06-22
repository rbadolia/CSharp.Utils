using System.Collections.Generic;

namespace CSharp.Utils.Contracts
{
    public interface ICompositeControllable : IControllable
    {
        #region Public Properties

        IList<IControllable> Controllables { get; }

        #endregion Public Properties
    }
}
