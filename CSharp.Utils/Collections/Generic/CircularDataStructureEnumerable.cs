using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class CircularDataStructureEnumerable<T> : IEnumerable<T>
        where T : class
    {
        #region Fields

        private readonly bool isCycleCompleted;

        private readonly int lastWrittenIndex;

        private readonly int startIndex;

        private readonly IDataStructureAdapter<T> store;

        #endregion Fields

        #region Constructors and Finalizers

        public CircularDataStructureEnumerable(IDataStructureAdapter<T> store, int startIndex, int lastWrittenIndex, bool isCycleCompleted)
        {
            this.store = store;
            this.isCycleCompleted = isCycleCompleted;
            this.startIndex = startIndex;
            this.lastWrittenIndex = lastWrittenIndex;
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

        protected virtual IEnumerator<T> GetEnumeratorCore()
        {
            return new CircularDataStructureEnumerator<T>(this.store, this.startIndex, this.lastWrittenIndex, this.isCycleCompleted);
        }

        #endregion Methods
    }
}
