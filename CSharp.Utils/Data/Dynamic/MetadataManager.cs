using System;

namespace CSharp.Utils.Data.Dynamic
{
    public static class MetadataManager
    {
        #region Constructors and Finalizers

        static MetadataManager()
        {
            MetadataProvider = new AttributeBasedMetadataProvider();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static IMetadataProvider MetadataProvider { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static IDbTableMetadata GetMetaData(Type entityType)
        {
            return MetadataProvider.GetMetaData(entityType);
        }

        #endregion Public Methods and Operators
    }
}
