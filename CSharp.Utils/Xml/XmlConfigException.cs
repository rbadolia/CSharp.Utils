using System;
using System.Runtime.Serialization;

namespace CSharp.Utils.Xml
{
    [Serializable]
    public class XmlConfigException : Exception
    {
        #region Constructors and Finalizers

        public XmlConfigException(string message, Exception e)
            : base(message, e)
        {
        }

        public XmlConfigException()
        {
        }

        public XmlConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal XmlConfigException(string message)
            : base(message)
        {
        }

        #endregion Constructors and Finalizers
    }
}
