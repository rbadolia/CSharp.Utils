using System.Xml;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Configuration
{
    public interface IConfigurable
    {
        #region Public Methods and Operators

        void Configure(XmlNode configurationNode, IObjectInstantiator instantiator = null);

        #endregion Public Methods and Operators
    }
}
