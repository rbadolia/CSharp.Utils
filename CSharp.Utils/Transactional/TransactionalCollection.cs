using System.Collections;
using System.Collections.Generic;

namespace CSharp.Utils.Transactional
{
    public abstract class TransactionalCollection<TC, T> : Transactional<TC>, IEnumerable<T>
        where TC : IEnumerable<T>
    {
        public TransactionalCollection(TC collection)
        {
            Value = collection;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<T> enumerable = this;
            return enumerable.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Value.GetEnumerator();
        }
    }
}
