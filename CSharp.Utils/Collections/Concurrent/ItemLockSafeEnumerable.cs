using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class ItemLockSafeEnumerable<T> : IEnumerable<T>
        where T : class
    {
        #region Fields

        private readonly IEnumerable<T> enumerable;

        #endregion Fields

        #region Constructors and Finalizers

        public ItemLockSafeEnumerable(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetEnumeratorCore();
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorCore();
        }

        #endregion Explicit Interface Methods

        #region Methods

        private IEnumerator<T> GetEnumeratorCore()
        {
            return new ItemLockSafeEnumerator(this.enumerable.GetEnumerator());
        }

        #endregion Methods

        private sealed class ItemLockSafeEnumerator : AbstractDisposable, IEnumerator<T>
        {
            #region Fields

            private readonly IEnumerator<T> enumerator;

            private readonly T previous = null;

            #endregion Fields

            #region Constructors and Finalizers

            public ItemLockSafeEnumerator(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
            }

            ~ItemLockSafeEnumerator()
            {
                this.Dispose(false);
            }

            #endregion Constructors and Finalizers

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public bool MoveNext()
            {
                bool canMove = this.enumerator.MoveNext();
                if (this.previous != null)
                {
                    Monitor.Exit(this.previous);
                }

                if (canMove)
                {
                    Monitor.Enter(this.enumerator.Current);
                }

                return canMove;
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            #endregion Public Methods and Operators

            #region Methods

            protected override void Dispose(bool disposing)
            {
                if (this.enumerator.Current != null)
                {
                    Monitor.Exit(this.enumerator.Current);
                }

                this.enumerator.Dispose();
            }

            #endregion Methods
        }
    }
}
