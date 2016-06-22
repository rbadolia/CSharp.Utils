using System.Data;
using CSharp.Utils.Data.Common;

namespace CSharp.Utils.Data.SqlServer
{
    public class DummyConnection : AbstractDummyConnection
    {
        #region Public Methods and Operators

        public override IDbCommand CreateCommand()
        {
            var command = new DummyCommand { Connection = this };
            return command;
        }

        #endregion Public Methods and Operators
    }
}
