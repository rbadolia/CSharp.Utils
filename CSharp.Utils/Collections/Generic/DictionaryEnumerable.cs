using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class DictionaryEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        #region Fields

        private readonly EnterOrExitReadLock _callback;

        private readonly IDictionary<TKey, TValue> _dictionary;

        #endregion Fields

        #region Constructors and Finalizers

        public DictionaryEnumerable(IDictionary<TKey, TValue> dictionary, EnterOrExitReadLock callback)
        {
            this._dictionary = dictionary;
            this._callback = callback;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this._dictionary, this._callback);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this._dictionary, this._callback);
        }

        #endregion Explicit Interface Methods
    }
}
