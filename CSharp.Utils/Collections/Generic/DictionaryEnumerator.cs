using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{

    #region Delegates

    public delegate void EnterOrExitReadLock(bool isEnter);

    #endregion Delegates

    public sealed class DictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        #region Fields

        private readonly EnterOrExitReadLock _callback;

        private readonly IDictionary<TKey, TValue> _dictionary;

        private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

        #endregion Fields

        #region Constructors and Finalizers

        public DictionaryEnumerator(IDictionary<TKey, TValue> dictionary, EnterOrExitReadLock callback)
        {
            this._dictionary = dictionary;
            this._callback = callback;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                return this._enumerator.Current;
            }
        }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                return this._enumerator.Current;
            }
        }

        #endregion Explicit Interface Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            this._callback(false);
            this._enumerator = null;
        }

        public bool MoveNext()
        {
            if (this._enumerator == null)
            {
                this._callback(true);
            }

            this._enumerator = this._dictionary.GetEnumerator();
            return this._enumerator.MoveNext();
        }

        public void Reset()
        {
            this._enumerator = null;
        }

        #endregion Public Methods and Operators
    }
}
