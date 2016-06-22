using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class BoundedEnumerable<T> : IEnumerable<T>
        where T : class
    {
        #region Fields

        private readonly int endIndex;

        private readonly int startIndex;

        private readonly IDataStructureAdapter<T> store;

        #endregion Fields

        #region Constructors and Finalizers

        public BoundedEnumerable(IDataStructureAdapter<T> store, int startIndex, int endIndex)
        {
            this.store = store;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public virtual IEnumerator<T> GetEnumerator()
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

        protected virtual IEnumerator<T> GetEnumeratorCore()
        {
            return new DataStructureEnumerator<T>(this.store, this.startIndex, this.endIndex);
        }

        #endregion Methods
    }
}
