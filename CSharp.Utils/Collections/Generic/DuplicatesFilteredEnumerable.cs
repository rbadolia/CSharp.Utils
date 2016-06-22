using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public sealed class DuplicatesFilteredEnumerable<T> : IEnumerable<T>
    {
        #region Fields

        private readonly IEnumerable<T> adaptedObject;

        private readonly IEqualityComparer<T> _comparer;

        #endregion Fields

        #region Constructors and Finalizers

        public DuplicatesFilteredEnumerable(IEnumerable<T> adaptedObject)
        {
            Guard.ArgumentNotNull(adaptedObject, "adaptedObject");
            this.adaptedObject = adaptedObject;
        }

        public DuplicatesFilteredEnumerable(IEnumerable<T> adaptedObject, IEqualityComparer<T> comparer)
            : this(adaptedObject)
        {
            Guard.ArgumentNotNull(comparer, "comparer");
            this._comparer = comparer;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            if (this._comparer == null)
            {
                return new DuplicatesFilteredEnumerator<T>(this.adaptedObject.GetEnumerator());
            }

            return new DuplicatesFilteredEnumerator<T>(this.adaptedObject.GetEnumerator(), this._comparer);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion Explicit Interface Methods
    }
}
