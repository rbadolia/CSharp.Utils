using System.IO;

namespace CSharp.Utils.Xml
{
    public interface IXmlExportable
    {
        #region Public Methods and Operators

        void WriteXml(TextWriter writer, string alignmentPrefix);

        #endregion Public Methods and Operators
    }
}
