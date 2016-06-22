using System.IO;
using System.Xml;

namespace CSharp.Utils.Xml
{
    public class RemovePrefixXmlTextReader : XmlTextReader
    {
        #region Fields

        private readonly XmlReaderSettings _settings;

        #endregion Fields

        #region Constructors and Finalizers

        public RemovePrefixXmlTextReader(string fileName, XmlReaderSettings settings)
            : base(File.Open(fileName, FileMode.Open))
        {
            this._settings = settings;
        }

        #endregion Constructors and Finalizers
        #region Public Properties

        public override string Prefix
        {
            get
            {
                return string.Empty;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this._settings;
            }
        }

        #endregion Public Properties
    }
}
