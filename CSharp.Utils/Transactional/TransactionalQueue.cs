using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Transactional
{
    public class TransactionalQueue<T> : TransactionalCollection<Queue<T>, T>, ICollection
    {
        public TransactionalQueue()
        : this(0)
        {
        }

        public TransactionalQueue(IEnumerable<T> collection)
        : base(new Queue<T>(collection))
        {
        }

        public TransactionalQueue(int capacity)
        : base(new Queue<T>(capacity))
        {
        }

        public int Count
        {
            get
            {
                return Value.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public T Dequeue()
        {
            return Value.Dequeue();
        }

        public void Enqueue(T item)
        {
            Value.Enqueue(item);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            (Value as ICollection).CopyTo(array, arrayIndex);
        }

        public T Peek()
        {
            return Value.Peek();
        }

        public T[] ToArray()
        {
            return Value.ToArray();
        }
    }
}
