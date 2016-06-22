using System.Collections.Generic;

namespace CSharp.Utils.Transactional
{
    public class TransactionalDictionary<TK, T> : TransactionalCollection<Dictionary<TK, T>, KeyValuePair<TK, T>>, IDictionary<TK, T>
    {
        public TransactionalDictionary()
        : this(0)
        {
        }

        public TransactionalDictionary(IDictionary<TK, T> dictionary)
        : base(new Dictionary<TK, T>(dictionary))
        {
        }

        public TransactionalDictionary(int capacity)
        : base(new Dictionary<TK, T>(capacity))
        {
        }

        public IEqualityComparer<TK> Comparer
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

        bool ICollection<KeyValuePair<TK, T>>.IsReadOnly
        {
            get
            {
                return (Value as ICollection<KeyValuePair<TK, T>>).IsReadOnly;
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

        public bool Remove(TK key)
        {
            return Value.Remove(key);
        }

        public bool TryGetValue(TK key, out T value)
        {
            return Value.TryGetValue(key, out value);
        }

        private Dictionary<TK, T>.Enumerator GetEnumerator()
        {
            return Value.GetEnumerator();
        }
    }
}
