using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public interface IDataSource<in T>
    {
        #region Public Methods and Operators

        void Populate(IEnumerable<T> dataTransferObjects, object tag = null);

        #endregion Public Methods and Operators
    }
}
