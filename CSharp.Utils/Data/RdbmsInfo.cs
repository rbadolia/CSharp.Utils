using System;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.Data
{
    public class RdbmsInfo : IUnique
    {
        #region Constructors and Finalizers

        public RdbmsInfo(string id, Type connectionType, Type bulkCopyType, string defaultConnectionString, Type dummyConnectionType)
        {
            this.Name = id;
            this.ConnectionType = connectionType;
            this.BulkCopyType = bulkCopyType;
            this.DefaultConnectionString = defaultConnectionString;
            this.DummyConnectionType = dummyConnectionType;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public Type BulkCopyType { get; private set; }

        public Type ConnectionType { get; private set; }

        public string DefaultConnectionString { get; private set; }

        public Type DummyConnectionType { get; private set; }

        public string Name { get; private set; }

        #endregion Public Properties
    }
}
