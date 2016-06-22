using System;
using System.Collections;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    public class DataStructureEnumerator<T> : AbstractDisposable, IKnownCountEnumerator<T>
    {
        #region Fields

        protected IDataStructureAdapter<T> _dataStructure;

        protected int _endIndex;

        protected int _indexInt = -1;

        protected int _startIndexInt;

        #endregion Fields
        #region Constructors and Finalizers

        public DataStructureEnumerator(IDataStructureAdapter<T> dataStructure, int startIndex, int endIndex)
        {
            this._dataStructure = dataStructure;
            this._startIndexInt = startIndex;
            this._endIndex = endIndex;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Count
        {
            get
            {
                return this._endIndex - this._startIndexInt;
            }
        }

        public T Current
        {
            get
            {
                return this._dataStructure[this._indexInt];
            }
        }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                return this._dataStructure[this._indexInt];
            }
        }

        #endregion Explicit Interface Properties

        #region Public Methods and Operators

        public virtual bool MoveNext()
        {
            this._indexInt++;
            return this._indexInt <= this._endIndex; // && _indexInt < _dataStructure.Count;
        }

        public void Reset()
        {
            this._indexInt = this._startIndexInt - 1;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
        }

        #endregion Methods
    }
}
