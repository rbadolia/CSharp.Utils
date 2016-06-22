using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class KeyValuePairKeyEnumerable<TKey, TValue> : IEnumerable<TKey>
    {
        #region Fields

        private readonly IEnumerable<KeyValuePair<TKey, TValue>> _pairs;

        #endregion Fields

        #region Constructors and Finalizers

        public KeyValuePairKeyEnumerable(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            this._pairs = pairs;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<TKey> GetEnumerator()
        {
            return new KeyValuePairKeyEnumerator(this._pairs.GetEnumerator());
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyValuePairKeyEnumerator(this._pairs.GetEnumerator());
        }

        #endregion Explicit Interface Methods

        private sealed class KeyValuePairKeyEnumerator : IEnumerator<TKey>
        {
            #region Fields

            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            #endregion Fields

            #region Constructors and Finalizers

            public KeyValuePairKeyEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this._enumerator = enumerator;
            }

            #endregion Constructors and Finalizers

            #region Public Properties

            public TKey Current
            {
                get
                {
                    return this._enumerator.Current.Key;
                }
            }

            #endregion Public Properties

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this._enumerator.Current.Key;
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public void Dispose()
            {
                this._enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return this._enumerator.MoveNext();
            }

            public void Reset()
            {
                this._enumerator.Reset();
            }

            #endregion Public Methods and Operators
        }
    }
}
