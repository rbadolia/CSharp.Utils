using System;
using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Runtime.Serialization.Formatters.Binary;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public sealed class CloneEnumerable<T> : IEnumerable<T>
    {
        #region Fields

        private readonly IEnumerable<T> adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        public CloneEnumerable(IEnumerable<T> adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            return new CloneEnumerator(this.adaptedObject.GetEnumerator());
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CloneEnumerator(this.adaptedObject.GetEnumerator());
        }

        #endregion Explicit Interface Methods

        private sealed class CloneEnumerator : IEnumerator<T>
        {
            #region Fields

            private readonly IEnumerator<T> adaptedObject;

            #endregion Fields

            #region Constructors and Finalizers

            public CloneEnumerator(IEnumerator<T> adaptedObject)
            {
                this.adaptedObject = adaptedObject;
            }

            #endregion Constructors and Finalizers

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this.getCurrent();
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    return this.getCurrent();
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public void Dispose()
            {
                this.adaptedObject.Dispose();
            }

            public bool MoveNext()
            {
                return this.adaptedObject.MoveNext();
            }

            public void Reset()
            {
                this.adaptedObject.Reset();
            }

            #endregion Public Methods and Operators

            #region Methods

            private T getCurrent()
            {
                T adaptedObjectCurrent = this.adaptedObject.Current;
                var deepCloneable = adaptedObjectCurrent as IDeepCloneable;
                if (deepCloneable != null)
                {
                    return (T)deepCloneable.DeepClone();
                }

                var cloneable = adaptedObjectCurrent as ICloneable;
                if (cloneable != null)
                {
                    return (T)cloneable.Clone();
                }

                return (T)adaptedObjectCurrent.DeepClone();
            }

            #endregion Methods
        }
    }
}
