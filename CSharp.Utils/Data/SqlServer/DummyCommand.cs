using System.Data;
using System.Data.SqlClient;
using CSharp.Utils.Data.Common;

namespace CSharp.Utils.Data.SqlServer
{
    public class DummyCommand : AbstractDummyCommand
    {
        #region Constructors and Finalizers

        public DummyCommand()
        {
            var comm = new SqlCommand();
            this.Parameters = comm.Parameters;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public override IDbDataParameter CreateParameter()
        {
            return new SqlParameter();
        }

        #endregion Public Methods and Operators
    }
}
