using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Transactional
{
    public class TransactionalSortedDictionary<TK, T> : TransactionalCollection<SortedDictionary<TK, T>, KeyValuePair<TK, T>>, IDictionary<TK, T>, IDictionary
    {
        public TransactionalSortedDictionary(IDictionary<TK, T> dictionary)
        : base(new SortedDictionary<TK, T>(dictionary))
        {
        }

        public TransactionalSortedDictionary(IDictionary<TK, T> dictionary, IComparer<TK> comparer)
        : base(new SortedDictionary<TK, T>(dictionary, comparer))
        {
        }

        public TransactionalSortedDictionary(IComparer<TK> comparer)
        : base(new SortedDictionary<TK, T>(comparer))
        {
        }

        public IComparer<TK> Comparer
        {
            get
            {
                return Value.Comparer;
            }
        }

        public int Count
        {
            get
            {
                return Value.Count;
            }
        }

        public ICollection<TK> Keys
        {
            get
            {
                return Value.Keys;
            }
        }

        public ICollection<T> Values
        {
            get
            {
                return Value.Values;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return (Value as ICollection).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return (Value as ICollection).SyncRoot;
            }
        }

        bool ICollection<KeyValuePair<TK, T>>.IsReadOnly
        {
            get
            {
                return (Value as ICollection<KeyValuePair<TK, T>>).IsReadOnly;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return (Value as IDictionary).IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return (Value as IDictionary).IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return (Value as IDictionary).Keys;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return (Value as IDictionary)[(TK)key];
            }

            set
            {
                (Value as SortedDictionary<TK, T>)[(TK)key] = (T)value;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return (Value as IDictionary).Values;
            }
        }

        public T this[TK key]
        {
            get
            {
                return Value[key];
            }

            set
            {
                Value[key] = value;
            }
        }

        public void Add(TK key, T item)
        {
            Value.Add(key, item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool ContainsKey(TK key)
        {
            return Value.ContainsKey(key);
        }

        public bool ContainsValue(T item)
        {
            return Value.ContainsValue(item);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            (Value as ICollection).CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<TK, T>>.Add(KeyValuePair<TK, T> item)
        {
            (Value as ICollection<KeyValuePair<TK, T>>).Add(item);
        }

        bool ICollection<KeyValuePair<TK, T>>.Contains(KeyValuePair<TK, T> item)
        {
            return (Value as ICollection<KeyValuePair<TK, T>>).Contains(item);
        }

        void ICollection<KeyValuePair<TK, T>>.CopyTo(KeyValuePair<TK, T>[] array, int arrayIndex)
        {
            (Value as ICollection<KeyValuePair<TK, T>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TK, T>>.Remove(KeyValuePair<TK, T> item)
        {
            return (Value as ICollection<KeyValuePair<TK, T>>).Remove(item);
        }

        void IDictionary.Add(object key, object value)
        {
            (Value as IDictionary<TK, T>).Add((TK)key, (T)value);
        }

        bool IDictionary.Contains(object key)
        {
            return (Value as IDictionary<TK, T>).ContainsKey((TK)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (Value as IDictionary).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            (Value as IDictionary<TK, T>).Remove((TK)key);
        }

        public bool Remove(TK key)
        {
            return Value.Remove(key);
        }

        public bool TryGetValue(TK key, out T value)
        {
            return Value.TryGetValue(key, out value);
        }

        private void CopyTo(KeyValuePair<TK, T>[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        private SortedDictionary<TK, T>.Enumerator GetEnumerator()
        {
            return Value.GetEnumerator();
        }
    }
}
