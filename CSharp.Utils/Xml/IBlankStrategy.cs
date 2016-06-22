using System.Xml;

namespace CSharp.Utils.Xml
{
    public interface IBlankStrategy
    {
        #region Public Methods and Operators

        bool IsBlank(XmlAttribute attribute);

        #endregion Public Methods and Operators
    }
}
