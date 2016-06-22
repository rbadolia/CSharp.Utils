using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class KeyValuePairValueEnumerable<TKey, TValue> : IEnumerable<TValue>
    {
        #region Fields

        private readonly IEnumerable<KeyValuePair<TKey, TValue>> _pairs;

        #endregion Fields

        #region Constructors and Finalizers

        public KeyValuePairValueEnumerable(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            this._pairs = pairs;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<TValue> GetEnumerator()
        {
            return new KeyValuePairValueEnumerator(this._pairs.GetEnumerator());
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyValuePairValueEnumerator(this._pairs.GetEnumerator());
        }

        #endregion Explicit Interface Methods

        private sealed class KeyValuePairValueEnumerator : IEnumerator<TValue>
        {
            #region Fields

            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            #endregion Fields

            #region Constructors and Finalizers

            public KeyValuePairValueEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this._enumerator = enumerator;
            }

            #endregion Constructors and Finalizers

            #region Public Properties

            public TValue Current
            {
                get
                {
                    return this._enumerator.Current.Value;
                }
            }

            #endregion Public Properties

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this._enumerator.Current.Value;
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
