using System;
using System.Xml.Serialization;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    [Serializable]
    [XmlRoot("Mapping")]
    public sealed class TypeMapping
    {
        #region Constructors and Finalizers

        public TypeMapping()
        {
        }

        public TypeMapping(string serverType, string clientType)
        {
            this.ServerType = serverType;
            this.ClientType = clientType;
        }

        public TypeMapping(string serverType, string clientType, string serverMethodName)
            : this(serverType, clientType)
        {
            this.ServerMethodName = serverMethodName;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        [XmlAttribute]
        public string ClientType { get; set; }

        [XmlAttribute]
        public string ServerMethodName { get; set; }

        [XmlAttribute]
        public string ServerType { get; set; }

        #endregion Public Properties
    }
}
