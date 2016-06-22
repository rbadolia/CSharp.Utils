using System.Configuration;
using System.Xml;
using CSharp.Utils.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Configuration
{
    public class ComponentsConfigurationSection : ConfigurationSection
    {
        private XmlDocument _document;

        private bool _areObjectsBuilt;

        private bool _isDocumentBuilt;

        private object[] _objects;

        private IObjectInstantiator _objectInstantiator;

        #region Methods

        protected override void DeserializeSection(XmlReader reader)
        {
            if (!this._isDocumentBuilt)
            {
                this._document = new XmlDocument();
                this._document.Load(reader);
                this._isDocumentBuilt = true;
            }
        }

        #region Public Properties

        public object[] GetObjects()
        {

            if (!this._areObjectsBuilt)
            {
                this._objects = ComponentBuilder.BuildComponentsFromXmlNode(this._document.ChildNodes[0], 
                    this._objectInstantiator);
                this._areObjectsBuilt = true;
            }

            return this._objects;
        }

        #endregion Public Properties

        public void SetObjectInstantiator(IObjectInstantiator objectInstantiator)
        {
            Guard.ArgumentNotNull(objectInstantiator, "objectInstantiator");
            this._objectInstantiator = objectInstantiator;
        }

        #endregion Methods
    }
}
