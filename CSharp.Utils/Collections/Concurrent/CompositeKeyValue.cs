using System.Collections.Generic;
using System.Xml;

namespace CSharp.Utils.Collections.Concurrent
{
    public sealed class CompositeKeyValue<TKey, TValue>
    {
        #region Constructors and Finalizers

        public CompositeKeyValue()
        {
        }

        public CompositeKeyValue(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public CompositeKeyValue(KeyValuePair<TKey, TValue> kvp)
        {
            this.Key = kvp.Key;
            this.Value = kvp.Value;
        }

        public CompositeKeyValue(TKey key, TValue value, List<CompositeKeyValue<TKey, TValue>> innerKeyValues)
        {
            this.Key = key;
            this.Value = value;
            this.InnerKeyValues = innerKeyValues;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public List<CompositeKeyValue<TKey, TValue>> InnerKeyValues { get; set; }

        public TKey Key { get; set; }

        public CompositeKeyValue<TKey, TValue> Parent { get; set; }

        public TValue Value { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static XmlDocument ExportAsXmlDocument(IEnumerable<CompositeKeyValue<TKey, TValue>> items)
        {
            var document = new XmlDocument();
            ExportToXmlDocument(items, document, document);
            return document;
        }

        public string GetPath(string separator)
        {
            CompositeKeyValue<TKey, TValue> node = this;
            string path = null;
            while (node != null)
            {
                path = node.Key + separator + path;
                node = node.Parent;
            }

            if (!string.IsNullOrEmpty(separator))
            {
                return path.Substring(0, path.Length - separator.Length);
            }

            return path;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void ExportToXmlDocument(IEnumerable<CompositeKeyValue<TKey, TValue>> items, XmlNode parentNode, XmlDocument document)
        {
            XmlNode itemsNode = document.CreateElement("Items");
            parentNode.AppendChild(itemsNode);
            foreach (var item in items)
            {
                ExportToXmlDocument(item, itemsNode, document);
            }
        }

        private static void ExportToXmlDocument(CompositeKeyValue<TKey, TValue> item, XmlNode parentNode, XmlDocument document)
        {
            XmlNode itemNode = document.CreateElement("Item");
            XmlAttribute keyAttribute = document.CreateAttribute("Key");
            keyAttribute.Value = item.Key.ToString();
            itemNode.Attributes.Append(keyAttribute);

            if (item.Value != null)
            {
                XmlAttribute valueAttribute = document.CreateAttribute("Value");
                valueAttribute.Value = item.Value.ToString();
                itemNode.Attributes.Append(valueAttribute);
            }

            parentNode.AppendChild(itemNode);
            if (item.InnerKeyValues != null)
            {
                ExportToXmlDocument(item.InnerKeyValues, itemNode, document);
            }
        }

        #endregion Methods
    }
}
