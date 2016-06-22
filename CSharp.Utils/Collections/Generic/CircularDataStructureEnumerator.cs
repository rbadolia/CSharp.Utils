using System;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class CircularDataStructureEnumerator<T> : DataStructureEnumerator<T>
        where T : class
    {
        #region Fields

        protected IDataStructureAdapter<T> circularDataStructure;

        protected int lastWrittenIndex = -1;

        private bool inCurrentIndexCrossed;

        #endregion Fields

        #region Constructors and Finalizers

        public CircularDataStructureEnumerator(IDataStructureAdapter<T> store, int startIndex, int lastWrittenIndex, bool isCycleCompleted)
            : base(store, startIndex, lastWrittenIndex)
        {
            this.circularDataStructure = store;
            this.lastWrittenIndex = lastWrittenIndex;
            this._indexInt = isCycleCompleted ? this.lastWrittenIndex - 1 : startIndex - 1;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public override bool MoveNext()
        {
            if ((this._indexInt == -1 && this.lastWrittenIndex == -1) || this.inCurrentIndexCrossed)
            {
                return false;
            }

            this._indexInt = (this._indexInt + 1) % this._dataStructure.Capacity;
            if (this._indexInt == this.lastWrittenIndex && !this.inCurrentIndexCrossed)
            {
                this.inCurrentIndexCrossed = true;
            }

            return true;
        }

        #endregion Public Methods and Operators
    }
}
