using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class ListAdapter<T> : IDataStructureAdapter<T>
    {
        #region Fields

        protected IList<T> adaptedObject;

        protected int _capacityInt = -1;

        #endregion Fields

        #region Constructors and Finalizers

        public ListAdapter(IList<T> adaptedObject)
        {
            this.adaptedObject = adaptedObject;
            this._capacityInt = this.adaptedObject.Count;
        }

        public ListAdapter(IList<T> adaptedObject, int capacity)
        {
            this.adaptedObject = adaptedObject;
            this._capacityInt = capacity;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public virtual int Capacity
        {
            get
            {
                return this._capacityInt;
            }
        }

        public int Count
        {
            get
            {
                return this.adaptedObject.Count;
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public T this[int index]
        {
            get
            {
                return this.adaptedObject.Count > index ? this.adaptedObject[index] : default(T);
            }

            set
            {
                if (this.adaptedObject.Count > index)
                {
                    this.adaptedObject[index] = value;
                }
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public void Clear()
        {
            this.adaptedObject.Clear();
        }

        #endregion Public Methods and Operators
    }
}
