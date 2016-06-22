using System.Data;

namespace CSharp.Utils.Data
{
    public interface IDbActivityStrategy
    {
        #region Public Methods and Operators

        object CreateMessageFromRecord(IDataRecord record);

        void InitializeCommandForDml(IDbCommand command, CudOperationType dmlOperationType, object message);

        void InitializeCommandForSelect(IDbCommand command, string clause);

        #endregion Public Methods and Operators
    }
}
