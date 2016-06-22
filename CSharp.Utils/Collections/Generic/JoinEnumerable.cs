using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public sealed class JoinEnumerable<T> : IEnumerable<T>
    {
        #region Fields

        private readonly IEnumerable<IEnumerable<T>> _enumerables;

        #endregion Fields

        #region Constructors and Finalizers

        public JoinEnumerable(IEnumerable<IEnumerable<T>> enumerables)
        {
            this._enumerables = enumerables;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            return new JoinEnumerator(this.BuildEnumerators());
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new JoinEnumerator(this.BuildEnumerators());
        }

        #endregion Explicit Interface Methods

        #region Methods

        private List<IEnumerator<T>> BuildEnumerators()
        {
            var enumerators = new List<IEnumerator<T>>();
            foreach (var enumerable in this._enumerables)
            {
                IEnumerator<T> enumerator = enumerable.GetEnumerator();
                enumerators.Add(enumerator);
            }

            return enumerators;
        }

        #endregion Methods

        private sealed class JoinEnumerator : IEnumerator<T>
        {
            #region Fields

            private readonly List<IEnumerator<T>> _enumerators;

            private int _currentIndex;

            #endregion Fields

            #region Constructors and Finalizers

            public JoinEnumerator(List<IEnumerator<T>> enumerators)
            {
                this._enumerators = enumerators;
            }

            #endregion Constructors and Finalizers

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this._enumerators[this._currentIndex].Current;
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    return this._enumerators[this._currentIndex].Current;
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._enumerators[this._currentIndex].MoveNext())
                {
                    return true;
                }

                this._enumerators[this._currentIndex].Dispose();
                this._currentIndex++;
                if (this._currentIndex >= this._enumerators.Count)
                {
                    return false;
                }

                return this.MoveNext();
            }

            public void Reset()
            {
            }

            #endregion Public Methods and Operators
        }
    }
}
