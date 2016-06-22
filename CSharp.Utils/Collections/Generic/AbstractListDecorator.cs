using System;
using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public abstract class AbstractListDecorator<T> : AbstractCollectionDecorator<T>, IList<T>, IList
    {
        private readonly IList<T> adaptedList;

        protected AbstractListDecorator(IList<T> adaptedList)
            : base(adaptedList)
        {
            Guard.ArgumentNotNull(adaptedList, "adaptedList");
            this.adaptedList = adaptedList;
        }

        protected IList<T> AdaptedList
        {
            get
            {
                return this.adaptedList;
            }
        }

        public virtual int IndexOf(T item)
        {
            return this.adaptedList.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            this.adaptedList.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            this.adaptedList.RemoveAt(index);
        }

        public virtual T this[int index]
        {
            get
            {
                return this.adaptedList[index];
            }

            set
            {
                this.adaptedList[index] = value;
            }
        }

        #region IList Members

        int IList.Add(object value)
        {
            this.Add((T)value);
            return this.Count - 1;
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.adaptedList is Array;
            }
        }

        bool IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((T[])array, index);
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                var list = this.adaptedList as IList;
                if (list != null)
                {
                    return list.IsSynchronized;
                }

                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                var list = this.adaptedList as IList;
                if (list != null)
                {
                    return list.SyncRoot;
                }

                return null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IList Members
    }
}
