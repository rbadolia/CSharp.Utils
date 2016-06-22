using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Transactional
{
    public class TransactionalStack<T> : TransactionalCollection<Stack<T>, T>, ICollection
    {
        public TransactionalStack()
        : this(0)
        {
        }

        public TransactionalStack(IEnumerable<T> collection)
        : base(new Stack<T>(collection))
        {
        }

        public TransactionalStack(int capacity)
        : base(new Stack<T>(capacity))
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

        public Stack<T>.Enumerator GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            (Value as ICollection).CopyTo(array, arrayIndex);
        }

        public T Peek()
        {
            return Value.Peek();
        }

        public T Pop()
        {
            return Value.Pop();
        }

        public void Push(T item)
        {
            Value.Push(item);
        }

        public T[] ToArray()
        {
            return Value.ToArray();
        }

        public void TrimExcess()
        {
            Value.TrimExcess();
        }

        private void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }
    }
}
