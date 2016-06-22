using System.Collections.Generic;

namespace CSharp.Utils.Data.Dynamic
{
    public class DbTableMetadata : IDbTableMetadata
    {
        #region Constructors and Finalizers

        public DbTableMetadata(string tableName, IList<IDbColumnMetadata> columnsMetaData)
        {
            this.TableName = tableName;
            this.ColumnsMetaData = columnsMetaData;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public IList<IDbColumnMetadata> ColumnsMetaData { get; private set; }

        public string TableName { get; private set; }

        #endregion Public Properties
    }
}
