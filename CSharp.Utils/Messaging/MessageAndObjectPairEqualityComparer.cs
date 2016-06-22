using System.Collections.Generic;
using System.Messaging;

namespace CSharp.Utils.Messaging
{
    public sealed class MessageAndObjectPairEqualityComparer<T> : IEqualityComparer<KeyValuePair<Message, T>>
    {
        #region Public Methods and Operators

        public bool Equals(KeyValuePair<Message, T> x, KeyValuePair<Message, T> y)
        {
            return x.Value.Equals(y.Value);
        }

        public int GetHashCode(KeyValuePair<Message, T> obj)
        {
            return obj.Value.GetHashCode();
        }

        #endregion Public Methods and Operators
    }
}
