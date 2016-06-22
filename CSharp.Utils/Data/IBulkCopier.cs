using System.Data;

namespace CSharp.Utils.Data
{
    public interface IBulkCopier
    {
        #region Public Methods and Operators

        void DoBulkCopy(IDataReader reader, IDbConnection connection, string tableName);

        void DoBulkCopy(IDataReader reader, IDbTransaction transaction, string tableName);

        #endregion Public Methods and Operators
    }
}
