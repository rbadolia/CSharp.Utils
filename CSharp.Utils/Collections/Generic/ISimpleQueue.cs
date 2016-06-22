using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public interface ISimpleQueue<T>
    {
        #region Public Methods and Operators

        IEnumerable<T> Dequeue();

        void Enqueue(T obj);

        void Enqueue(IEnumerable<T> objects);

        #endregion Public Methods and Operators
    }
}
