using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public sealed class DuplicatesFilteredEnumerator<T> : IEnumerator<T>
    {
        #region Fields

        private readonly IEnumerator<T> adaptedObject;

        private readonly HashSet<T> _hashSet;

        #endregion Fields

        #region Constructors and Finalizers

        public DuplicatesFilteredEnumerator(IEnumerator<T> adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
            this._hashSet = new HashSet<T>();
        }

        public DuplicatesFilteredEnumerator(IEnumerator<T> adaptedObject, IEqualityComparer<T> comparer)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            Guard.ArgumentNotNull(comparer, "comparer");
            this.adaptedObject = adaptedObject;
            this._hashSet = new HashSet<T>(comparer);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public T Current
        {
            get
            {
                return this.adaptedObject.Current;
            }
        }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                return this.adaptedObject.Current;
            }
        }

        #endregion Explicit Interface Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            this.adaptedObject.Dispose();
            this._hashSet.Clear();
        }

        public bool MoveNext()
        {
            while (this.adaptedObject.MoveNext())
            {
                if (this._hashSet.Add(this.adaptedObject.Current))
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            this.adaptedObject.Reset();
            this._hashSet.Clear();
        }

        #endregion Public Methods and Operators
    }
}
