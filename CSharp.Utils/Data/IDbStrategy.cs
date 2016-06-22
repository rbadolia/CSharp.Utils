using System.Data;

namespace CSharp.Utils.Data
{
    public interface IDbStrategy
    {
        #region Public Properties

        IBulkCopier BulkCopier { get; }

        #endregion Public Properties

        #region Public Methods and Operators

        IDbConnection CreateConnection();

        IDbConnection CreateDummyConnection();

        #endregion Public Methods and Operators
    }
}
