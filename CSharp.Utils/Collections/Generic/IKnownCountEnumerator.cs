using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public interface IKnownCountEnumerator<out T> : IEnumerator<T>
    {
        #region Public Properties

        int Count { get; }

        #endregion Public Properties
    }
}
