using System.Collections.Generic;

namespace CSharp.Utils.Data.Dynamic
{
    public interface IDbTableMetadata
    {
        #region Public Properties

        IList<IDbColumnMetadata> ColumnsMetaData { get; }

        string TableName { get; }

        #endregion Public Properties
    }
}
