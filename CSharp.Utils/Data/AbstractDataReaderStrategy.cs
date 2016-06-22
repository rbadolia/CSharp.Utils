using System;
using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public abstract class AbstractDataReaderStrategy<T> : AbstractInitializableAndDisposable, IDataReaderStrategy
    {
        #region Fields

        private List<string> _columnNames;

        private IList<KeyValuePair<string, Type>> _columns;

        private object[] _itemArray;

        #endregion Fields

        #region Public Properties

        public ICollection<string> ColumnNames
        {
            get
            {
                return this._columnNames;
            }
        }

        public long TotalRecordCountIfKnown { get; protected set; }

        #endregion Public Properties

        #region Public Indexers

        public object this[int i]
        {
            get
            {
                return this._itemArray[i];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public Type GetFieldType(int i)
        {
            return this._columns[i].Value;
        }

        public bool Read()
        {
            T current = this.readNextObject();
            if (current != null)
            {
                this.populateItemArray(current, this._itemArray);
                for (int i = 0; i < this._itemArray.Length; i++)
                {
                    if (this._itemArray[i] == null)
                    {
                        this._itemArray[i] = DBNull.Value;
                    }
                }

                return true;
            }

            return false;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected abstract IList<KeyValuePair<string, Type>> getColumns();

        protected override void InitializeProtected()
        {
            this._columns = this.getColumns();
            this._itemArray = new object[this._columns.Count];
            this._columnNames = new List<string>();
            foreach (var kvp in this._columns)
            {
                this._columnNames.Add(kvp.Key);
            }
        }

        protected abstract void populateItemArray(T obj, object[] itemArray);

        protected abstract T readNextObject();

        #endregion Methods
    }
}
