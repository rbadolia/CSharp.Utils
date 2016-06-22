using System.Collections;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public abstract class AbstractCollectionDecorator<T> : ICollection<T>
    {
        private readonly ICollection<T> adaptedCollection;

        protected AbstractCollectionDecorator(ICollection<T> adaptedCollection)
        {
            Guard.ArgumentNotNull(adaptedCollection, "adaptedCollection");
            this.adaptedCollection = adaptedCollection;
        }

        public virtual int Count
        {
            get
            {
                return this.adaptedCollection.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return this.adaptedCollection.IsReadOnly;
            }
        }

        protected ICollection<T> AdaptedCollection
        {
            get
            {
                return this.adaptedCollection;
            }
        }

        public virtual bool Remove(T item)
        {
            return this.adaptedCollection.Remove(item);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return this.adaptedCollection.GetEnumerator();
        }

        public virtual void Add(T item)
        {
            this.adaptedCollection.Add(item);
        }

        public virtual void Clear()
        {
            this.adaptedCollection.Clear();
        }

        public virtual bool Contains(T item)
        {
            return this.adaptedCollection.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            this.adaptedCollection.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.adaptedCollection.GetEnumerator();
        }
    }
}
