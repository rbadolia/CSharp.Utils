using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class InverseComparer<T> : IComparer<T>
    {
        #region Fields

        private readonly IComparer<T> adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        public InverseComparer(IComparer<T> adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public int Compare(T x, T y)
        {
            return -this.adaptedObject.Compare(x, y);
        }

        #endregion Public Methods and Operators
    }
}
