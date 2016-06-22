using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public sealed class CounterEnumeratorDecorator<T> : IEnumerator<T>
    {
        #region Fields

        private readonly IEnumerator<T> _enumerator;

        #endregion Fields

        #region Constructors and Finalizers

        public CounterEnumeratorDecorator(IEnumerator<T> enumerator)
        {
            this._enumerator = enumerator;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Current
        {
            get
            {
                this.RedCount++;
                return this._enumerator.Current;
            }
        }

        public int RedCount { get; private set; }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                this.RedCount++;
                return this._enumerator.Current;
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
