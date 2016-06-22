using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public interface IDataSink<in T>
    {
        #region Public Methods and Operators

        void InsertOrUpdate(IEnumerable<T> dataTransferObjects, object tag = null);

        #endregion Public Methods and Operators
    }
}
