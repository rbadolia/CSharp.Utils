using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CSharp.Utils.IO;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Xml
{
    public static class XmlHelper
    {
        #region Public Methods and Operators

        public static void ApplyAppKeyIfRequired(ref string value)
        {
            if (value != null && value.Length > 1 && (value[0] == '$' && value[value.Length - 1] == '$'))
            {
                value = ConfigurationManager.AppSettings[value.Substring(1, value.Length - 2)];
            }
        }

        public static XmlDocument BuildXmlDocumentFromFile(string fileName)
        {
            fileName = IOHelper.ResolvePath(fileName);
            var document = new XmlDocument();
            document.Load(fileName);
            return document;
        }

        public static XmlDocument BuildXmlDocumentFromXmlString(string xml, string name)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        public static XmlException CreateAttributeMustBeSpecifiedException(XmlNode node, string name)
        {
            string nodeName = node is XmlDocument ? "document" : node.Name;
            throw new XmlConfigException(string.Format("Attribute {0} from node {1} must be specified", name, nodeName));
        }

        public static XmlException CreateXmlConfigException(string message)
        {
            throw new XmlConfigException(message);
        }

        public static void FillTheMissingValues(XmlDocument targetDocument, XmlDocument referenceDocument)
        {
            FillTheMissingValues(targetDocument, referenceDocument, DefaultBlankStrategy.Instance);
        }

        public static void FillTheMissingValues(XmlDocument targetDocument, XmlDocument referenceDocument, IBlankStrategy strategy)
        {
        }

        public static string FormatXml(string xml)
        {
            XDocument xDocument = XDocument.Parse(xml);
            return xDocument.ToString();
        }

        public static bool? GetBoolAttribute(XmlNode node, string name)
        {
            return GetBoolAttribute(node, name, false);
        }

        public static bool? GetBoolNode(XmlNode node, string name)
        {
            return GetBoolNode(node, name, false);
        }

        public static bool? GetBoolNode(XmlNode node, string name, bool mandatory)
        {
            XmlNode nodeFound = GetNode(node, name, mandatory);
            if (nodeFound == null)
            {
                return null;
            }

            try
            {
                return bool.Parse(nodeFound.InnerText);
            }
            catch (FormatException)
            {
                throw CreateNodeParseException(node, nodeFound, typeof(bool));
            }
        }

        public static XmlNodeList GetChildNodes(XmlNode node)
        {
            return node.ChildNodes;
        }

        public static decimal? GetDecimalAttribute(XmlNode node, string name)
        {
            return GetDecimalAttribute(node, name, false);
        }

        public static double? GetDoubleAttribute(XmlNode node, string name)
        {
            return GetDoubleAttribute(node, name, false);
        }

        public static int? GetIntAttribute(XmlNode node, string name)
        {
            return GetIntAttribute(node, name, false);
        }

        public static int? GetIntNode(XmlNode node, string name)
        {
            return GetIntNode(node, name, false);
        }

        public static long? GetLongAttribute(XmlNode node, string name)
        {
            return GetLongAttribute(node, name, false);
        }

        public static long? GetLongNode(XmlNode node, string name)
        {
            return GetLongNode(node, name, false);
        }

        public static bool GetMandatoryBoolAttribute(XmlNode node, string name)
        {
            return (bool)GetBoolAttribute(node, name, true);
        }

        public static bool GetMandatoryBoolNode(XmlNode node, string name)
        {
            return (bool)GetBoolNode(node, name, true);
        }

        public static decimal GetMandatoryDecimalAttribute(XmlNode node, string name)
        {
            return (decimal)GetIntAttribute(node, name, true);
        }

        public static double GetMandatoryDoubleAttribute(XmlNode node, string name)
        {
            return (double)GetDoubleAttribute(node, name, true);
        }

        public static int GetMandatoryIntAttribute(XmlNode node, string name)
        {
            return (int)GetIntAttribute(node, name, true);
        }

        public static int GetMandatoryIntNode(XmlNode node, string name)
        {
            return (int)GetIntNode(node, name, true);
        }

        public static long GetMandatoryLongAttribute(XmlNode node, string name)
        {
            return (long)GetLongAttribute(node, name, true);
        }

        public static long GetMandatoryLongNode(XmlNode node, string name)
        {
            return (long)GetLongNode(node, name, true);
        }

        public static XmlNode GetMandatoryNode(XmlNode node, string xpath)
        {
            return GetNode(node, xpath, true);
        }

        public static XmlNodeList GetMandatoryNodes(XmlNode node, string xpath)
        {
            return GetNodes(node, xpath, true);
        }

        public static string GetMandatoryStringAttribute(XmlNode node, string name)
        {
            return GetStringAttribute(node, name, true);
        }

        public static string GetMandatoryStringNode(XmlNode node, string xpath)
        {
            return GetStringNode(node, xpath, true);
        }

        public static XmlNode GetNode(XmlNode node, string xpath)
        {
            return GetNode(node, xpath, false);
        }

        public static string GetNodeContent(XmlNode node)
        {
            return node.InnerText;
        }

        public static XmlNode GetNodeFromFile(string fileName, string nodePath)
        {
            XmlDocument document = BuildXmlDocumentFromFile(fileName);
            XmlNode node = GetNode(document, nodePath);
            return node;
        }

        public static XmlNodeList GetNodes(XmlNode node, string xpath)
        {
            return GetNodes(node, xpath, false);
        }

        public static string GetStringAttribute(XmlNode node, string name)
        {
            return GetStringAttribute(node, name, false);
        }

        public static string GetStringNode(XmlNode node, string xpath)
        {
            return GetStringNode(node, xpath, false);
        }

        public static Type GetType(XmlNode node)
        {
            string typeName = GetStringAttribute(node, "type");
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            Type type = ReflectionHelper.GetType(typeName);
            if (type == null || !type.IsGenericType)
            {
                return type;
            }

            string genericName = GetStringAttribute(node, "generic");
            if (string.IsNullOrEmpty(genericName))
            {
                return type;
            }

            Type genericType = Type.GetType(genericName, true);
            var genericParam = new Type[1];
            genericParam[0] = genericType;
            return type.MakeGenericType(genericParam);
        }

        public static List<XmlNode> GetUncommentedChildNodes(XmlNodeList nodeList)
        {
            return nodeList.Cast<XmlNode>().Where(node => node.NodeType != XmlNodeType.XmlDeclaration && node.NodeType != XmlNodeType.Comment).ToList();
        }

        public static string GetXpath(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Attribute)
            {
                return string.Format("{0}/@{1}", GetXpath(((XmlAttribute)node).OwnerElement), node.Name);
            }

            if (node.ParentNode == null)
            {
                return string.Empty;
            }

            int iIndex = 1;
            XmlNode xnIndex = node;
            while (xnIndex.PreviousSibling != null && xnIndex.PreviousSibling.Name == xnIndex.Name)
            {
                iIndex++;
                xnIndex = xnIndex.PreviousSibling;
            }

            return string.Format("{0}/{1}[{2}]", GetXpath(node.ParentNode), node.Name, iIndex);
        }

        [CautionUsedByReflection]
        public static void WriteAttribute(string name, string value, XmlWriter writer)
        {
            if (value != null)
            {
                writer.WriteAttributeString(XmlConvert.EncodeName(name), value);
            }
        }

        public static Dictionary<string, string> XmlAttributeCollectionToDictionary(XmlAttributeCollection collection)
        {
            return collection.Cast<XmlAttribute>().ToDictionary(attrib => attrib.Name, attrib => attrib.Value);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static XmlConfigException CreateAttributeParseException(XmlNode node, string name, string attribute, Type type)
        {
            string nodeName = node is XmlDocument ? "document" : node.Name;
            return new XmlConfigException(string.Format("The value {0} of the attribute {1} of element {2} must be a {3}", attribute, name, nodeName, type));
        }

        private static XmlConfigException CreateNodeParseException(XmlNode parentNode, XmlNode xmlNode, Type type)
        {
            string nodeName = xmlNode is XmlDocument ? "document" : xmlNode.Name;
            string parentNodeName = parentNode is XmlDocument ? "document" : parentNode.Name;
            return new XmlConfigException(string.Format("The inner text {0} of the node {1} from parent node {2} must be a {3}", xmlNode.InnerText, nodeName, parentNodeName, type));
        }

        private static string GetAttribute(XmlNode node, string name, bool mandatory)
        {
            XmlAttribute attribute = node.Attributes != null ? node.Attributes[name] : null;
            string value = attribute != null ? attribute.Value.Trim() : null;
            ApplyAppKeyIfRequired(ref value);
            value = string.IsNullOrEmpty(value) ? null : value;
            if (mandatory && value == null)
            {
                throw CreateAttributeMustBeSpecifiedException(node, name);
            }

            return value;
        }

        private static bool? GetBoolAttribute(XmlNode node, string name, bool mandatory)
        {
            string value = GetAttribute(node, name, mandatory);

            try
            {
                return value != null ? bool.Parse(value) : (bool?)null;
            }
            catch (FormatException ex)
            {
                Debug.WriteLine(ex);
                throw CreateAttributeParseException(node, name, value, typeof(bool));
            }
        }

        private static decimal? GetDecimalAttribute(XmlNode node, string name, bool mandatory)
        {
            string value = GetAttribute(node, name, mandatory);
            try
            {
                return value != null ? decimal.Parse(value) : (decimal?)null;
            }
            catch (FormatException)
            {
                throw CreateAttributeParseException(node, name, value, typeof(decimal));
            }
        }

        private static double? GetDoubleAttribute(XmlNode node, string name, bool mandatory)
        {
            string value = GetAttribute(node, name, mandatory);
            try
            {
                return value != null ? double.Parse(value) : (double?)null;
            }
            catch (FormatException)
            {
                throw CreateAttributeParseException(node, name, value, typeof(double));
            }
        }

        private static int? GetIntAttribute(XmlNode node, string name, bool mandatory)
        {
            string value = GetAttribute(node, name, mandatory);
            try
            {
                return value != null ? int.Parse(value) : (int?)null;
            }
            catch (FormatException)
            {
                throw CreateAttributeParseException(node, name, value, typeof(int));
            }
        }

        private static int? GetIntNode(XmlNode node, string name, bool mandatory)
        {
            XmlNode nodeFound = GetNode(node, name, mandatory);
            if (nodeFound == null)
            {
                return null;
            }

            try
            {
                return int.Parse(nodeFound.InnerText);
            }
            catch (FormatException)
            {
                throw CreateNodeParseException(node, nodeFound, typeof(int));
            }
        }

        private static long? GetLongAttribute(XmlNode node, string name, bool mandatory)
        {
            string value = GetAttribute(node, name, mandatory);
            try
            {
                return value != null ? long.Parse(value) : (long?)null;
            }
            catch (FormatException)
            {
                throw CreateAttributeParseException(node, name, value, typeof(long));
            }
        }

        private static long? GetLongNode(XmlNode node, string name, bool mandatory)
        {
            XmlNode nodeFound = GetNode(node, name, mandatory);
            if (nodeFound == null)
            {
                return null;
            }

            try
            {
                return long.Parse(nodeFound.InnerText);
            }
            catch (FormatException)
            {
                throw CreateNodeParseException(node, nodeFound, typeof(long));
            }
        }

        private static XmlNode GetNode(XmlNode node, string xpath, bool mandatory)
        {
            XmlNodeList nodes = GetNodes(node, xpath, mandatory);
            if (nodes.Count > 1)
            {
                string nodeName = node is XmlDocument ? "document" : node.Name;
                throw new XmlConfigException(string.Format("Only one node path {0} from node {1} must be specified", xpath, nodeName));
            }

            return nodes[0];
        }

        private static XmlNodeList GetNodes(XmlNode node, string xpath, bool mandatory)
        {
            XmlNodeList xmlNodeList = node.SelectNodes(xpath);
            if (mandatory && (xmlNodeList == null || xmlNodeList.Count == 0))
            {
                string nodeName = node is XmlDocument ? "document" : node.Name;
                throw new XmlConfigException(string.Format("Nodes path {0} of node {1} must be specified", xpath, nodeName));
            }

            return xmlNodeList;
        }

        private static string GetStringAttribute(XmlNode node, string name, bool mandatory)
        {
            return GetAttribute(node, name, mandatory);
        }

        private static string GetStringNode(XmlNode node, string xpath, bool mandatory)
        {
            XmlNode nodeFound = GetNode(node, xpath, mandatory);
            if (nodeFound == null)
            {
                return null;
            }

            if (mandatory && string.IsNullOrEmpty(nodeFound.InnerText.Trim()))
            {
                string nodeName = node is XmlDocument ? "document" : node.Name;
                throw new XmlConfigException(string.Format("Node path {0} from node {1} must be not empty", xpath, nodeName));
            }

            return nodeFound.InnerText;
        }

        private static void fillTheMissingValues(XmlNode targetNode, XmlNode referenceNode, IBlankStrategy strategy)
        {
            if (targetNode.NodeType == XmlNodeType.Comment)
            {
                return;
            }

            for (int i = 0; i < targetNode.Attributes.Count; i++)
            {
                if (strategy.IsBlank(targetNode.Attributes[i]))
                {
                    string xpath = GetXpath(targetNode);
                    XmlNode node = GetNode(referenceNode, xpath);
                    Dictionary<string, string> dictionary = XmlAttributeCollectionToDictionary(node.Attributes);
                    if (dictionary != null)
                    {
                        string value;
                        if (dictionary.TryGetValue(targetNode.Attributes[i].Name, out value))
                        {
                            targetNode.Attributes[i].Value = value;
                        }
                    }
                }
            }

            foreach (XmlNode childNode in targetNode.ChildNodes)
            {
                fillTheMissingValues(childNode, referenceNode, strategy);
            }
        }

        #endregion Methods
    }
}
