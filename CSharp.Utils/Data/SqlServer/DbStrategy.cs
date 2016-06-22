using System.Data;
using System.Data.SqlClient;

namespace CSharp.Utils.Data.SqlServer
{
    public sealed class DbStrategy : IDbStrategy
    {
        #region Static Fields

        private static readonly DbStrategy InstanceObject = new DbStrategy();

        #endregion Static Fields

        #region Constructors and Finalizers

        private DbStrategy()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static DbStrategy Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public IBulkCopier BulkCopier
        {
            get
            {
                return SqlServer.BulkCopier.Instance;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public IDbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        public IDbConnection CreateDummyConnection()
        {
            return new DummyConnection();
        }

        #endregion Public Methods and Operators
    }
}
