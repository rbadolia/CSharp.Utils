using System;

namespace CSharp.Utils.Data.Dynamic
{
    public interface IMetadataProvider
    {
        #region Public Methods and Operators

        IDbTableMetadata GetMetaData(Type entityType);

        #endregion Public Methods and Operators
    }
}
