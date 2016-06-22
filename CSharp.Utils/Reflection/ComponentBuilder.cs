using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CSharp.Utils.Configuration;
using CSharp.Utils.IO;
using CSharp.Utils.Xml;

namespace CSharp.Utils.Reflection
{
    public static class ComponentBuilder
    {
        #region Public Methods and Operators

        public static object BuildComponent(XmlNode node, IObjectInstantiator instantiator = null, 
            bool forConfigCheckOnly = false)
        {
            object o = buildComponent(node, instantiator, forConfigCheckOnly);
            if (o == null)
            {
                throw new XmlConfigException(
                    "Could not identify the type or could not create instance from the given XmlNode \r\n" +
                    XmlHelper.GetXpath(node) + Environment.NewLine + node.OuterXml);
            }

            return o;
        }

        public static T BuildComponent<T>(XmlNode node, IObjectInstantiator instantiator = null, 
            bool forConfigCheckOnly = false) where T : class
        {
            return BuildComponent(node, instantiator, forConfigCheckOnly) as T;
        }

        public static object BuildComponentFromFile(string fileName, string configNodePath, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false)
        {
            fileName = IOHelper.ResolvePath(fileName);
            XmlNode node = XmlHelper.GetNodeFromFile(fileName, configNodePath);
            if (node != null)
            {
                return BuildComponent(node, instantiator, forConfigCheckOnly);
            }

            throw new XmlConfigException("There is no XmlNode with the given path " + configNodePath);
        }

        public static T BuildComponentFromFile<T>(string fileName, string nodePath, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false) where T : class
        {
            return BuildComponentFromFile(fileName, nodePath, instantiator, forConfigCheckOnly) as T;
        }

        public static object[] BuildComponentsFromFile(string fileName, IObjectInstantiator instantiator = null, 
            bool forConfigCheckOnly = false)
        {
            return BuildComponentsFromFile(fileName, "Configuration", instantiator, forConfigCheckOnly);
        }

        public static object[] BuildComponentsFromFile(string fileName, string configNodePath, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false)
        {
            fileName = IOHelper.ResolvePath(fileName);
            XmlNode node = XmlHelper.GetNodeFromFile(fileName, configNodePath);
            if (node != null)
            {
                return buildComponentsFromXmlNode(node, fileName, configNodePath, instantiator, forConfigCheckOnly);
            }

            throw new XmlConfigException("There is no XmlNode with the given path " + configNodePath);
        }

        public static object[] BuildComponentsFromFile(string fileName, bool autoUpdateOnFileChange, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false, params string[] nodesPath)
        {
            var objects = new object[nodesPath.Length];
            for (int i = 0; i < nodesPath.Length; i++)
            {
                XmlNode node = XmlHelper.GetNodeFromFile(fileName, nodesPath[i]);
                object component = BuildComponent(node, instantiator, forConfigCheckOnly);
                if (autoUpdateOnFileChange)
                {
                    var info = new ComponentUpdateRegistrationInfo(component, fileName, nodesPath[i], null, instantiator);
                    ComponentUpdateWatcher.RegisterForUpdate(info);
                }

                objects[i] = component;
            }

            return objects;
        }

        public static object[] BuildComponentsFromXmlDocument(XmlDocument document, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false)
        {
            return BuildComponentsFromXmlDocument(document, "Configuration", instantiator, forConfigCheckOnly);
        }

        public static object[] BuildComponentsFromXmlDocument(XmlDocument document, string configNodePath, 
            IObjectInstantiator instantiator = null, bool forConfigCheckOnly = false)
        {
            XmlNode node = XmlHelper.GetNode(document, configNodePath);
            return BuildComponentsFromXmlNode(node, instantiator, forConfigCheckOnly);
        }

        public static object[] BuildComponentsFromXmlNode(XmlNode node, IObjectInstantiator instantiator = null, 
            bool forConfigCheckOnly = false)
        {
            return buildComponentsFromXmlNode(node, null, null, instantiator, forConfigCheckOnly);
        }

        public static void PopulateComponentState(object o, XmlNode node, IObjectInstantiator instantiator = null)
        {
            populateComponentStatePrivate(o, node, instantiator, false);
        }

        public static void PopulateList<T>(List<T> list, XmlNode listNode, IObjectInstantiator instantiator = null)
        {
            populateListPrivate(list, typeof (T), listNode, instantiator, false);
        }

        public static void UpdateComponent(object component, string fileName, string nodePath, 
            IObjectInstantiator instantiator = null)
        {
            XmlNode node = XmlHelper.GetNodeFromFile(fileName, nodePath);
            UpdateComponent(component, node, instantiator);
        }

        public static void UpdateComponent(object component, XmlNode node, IObjectInstantiator instantiator = null)
        {
            Dictionary<string, PropertyInfo> propertiesDictionary = buildPropertiesDictionary(component.GetType());
            Dictionary<string, XmlAttribute> attributesDictionary = buildAttributesDictionary(node.Attributes);
            setAttributeProperties(attributesDictionary, propertiesDictionary, component, node, true);
            updateNodeProperties(propertiesDictionary, component, node, instantiator);
            performCheckAfterComponentConfig(attributesDictionary, component, node, false);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static Dictionary<string, XmlAttribute> buildAttributesDictionary(IEnumerable collection)
        {
            var dictionary = new Dictionary<string, XmlAttribute>();
            foreach (XmlAttribute v in collection)
            {
                if (v.Name != "type" && v.NodeType != XmlNodeType.Comment)
                {
                    dictionary.Add(v.Name, v);
                }
            }

            return dictionary;
        }

        private static object buildComponent(XmlNode node, IObjectInstantiator instantiator, 
            bool forConfigCheckOnly = false)
        {
            Type type = XmlHelper.GetType(node);
            if (type != null)
            {
                object o = instantiate(type, instantiator);
                if (o != null)
                {
                    var configurable = o as IConfigurable;
                    if (configurable != null)
                    {
                        configurable.Configure(node);
                    }
                    else
                    {
                        populateComponentStatePrivate(o, node, instantiator, forConfigCheckOnly);
                    }

                    return o;
                }
            }

            return null;
        }

        private static object buildComponent(XmlNode node, PropertyInfo property, IObjectInstantiator instantiator, 
            bool forConfigCheckOnly)
        {
            object o = buildComponent(node, instantiator, forConfigCheckOnly);
            if (o == null)
            {
                Type listType;
                Type[] genericParameters;
                bool isList = ReflectionHelper.CreateListIfPropertyTypeIsACollection(property.PropertyType, out listType, 
                    out genericParameters);
                if (!isList)
                {
                    o = buildObject(property.PropertyType, node, instantiator, forConfigCheckOnly);
                }
                else
                {
                    o = instantiate(listType, instantiator);
                    var list = o as IList;
                    populateListPrivate(list, genericParameters[0], node, instantiator, forConfigCheckOnly);
                }
            }

            return o;
        }

        private static object instantiate(Type objectType, IObjectInstantiator instantiator)
        {
            if (instantiator == null)
            {
                instantiator = ActivatorBasedObjectInstantiator.Instance;
            }

            object o = instantiator.Instantiate(objectType);
            return o;
        }

        private static object[] buildComponentsFromXmlNode(XmlNode node, string fileName, string configNodePath, IObjectInstantiator instantiator, bool forConfigCheckOnly)
        {
            var objects = new List<object>(node.ChildNodes.Count);
            foreach (XmlNode childNode in XmlHelper.GetUncommentedChildNodes(node.ChildNodes))
            {
                try
                {
                    object o = BuildComponent(childNode, instantiator, forConfigCheckOnly);
                    objects.Add(o);
                    if (fileName != null)
                    {
                        bool? autoUpdate = XmlHelper.GetBoolAttribute(childNode, "autoUpdate");
                        if (autoUpdate != null && autoUpdate.Value)
                        {
                            var info = new ComponentUpdateRegistrationInfo(o, fileName, configNodePath + "/" + childNode.Name, null, instantiator);
                            ComponentUpdateWatcher.RegisterForUpdate(info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw;
                }
            }

            return objects.ToArray();
        }

        private static object buildObject(Type type, XmlNode node, IObjectInstantiator instantiator, bool forConfigCheckOnly)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                throw new XmlConfigException("Cannot instantiate the type " + type + ". It is an interface or an abstract class");
            }

            object o = instantiate(type, instantiator);

            populateComponentStatePrivate(o, node, instantiator, forConfigCheckOnly);
            return o;
        }

        private static Dictionary<string, PropertyInfo> buildPropertiesDictionary(Type type)
        {
            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties(type, false);
            return properties.ToDictionary(v => v.Name);
        }

        private static void performCheckAfterComponentConfig(IDictionary<string, XmlAttribute> attributesDictionary, object obj, XmlNode node, bool forConfigCheckOnly)
        {
            XmlAttribute runMethodsAttribute;
            if (attributesDictionary.TryGetValue("runMethods", out runMethodsAttribute))
            {
                attributesDictionary.Remove("runMethods");
            }

            if (attributesDictionary.Count > 1 || (attributesDictionary.Count == 1 && !attributesDictionary.ContainsKey("autoUpdate")))
            {
                var sb = new StringBuilder();
                foreach (string key in attributesDictionary.Keys.ToArray())
                {
                    if (key != "autoUpdate")
                    {
                        sb.Append(key + ",");
                    }
                }

                sb.Length--;

                throw new XmlConfigException(@"There are one or more attributes which won't map to any _properties.\r\nThe attrubutes are " + sb + ".\r\n" + XmlHelper.GetXpath(node) + Environment.NewLine + node.OuterXml);
            }

            if (runMethodsAttribute == null)
            {
                return;
            }

            string[] methods = runMethodsAttribute.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < methods.Length; i++)
            {
                methods[i] = methods[i].Trim();
            }

            var filteredMethods = new MethodInfo[methods.Length];
            MethodInfo[] methodInfos = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            int foundCount = 0;
            foreach (MethodInfo method in methodInfos)
            {
                if (method.IsPublic && !method.IsStatic && !method.IsGenericMethod && method.GetParameters().Length == 0)
                {
                    int index = Array.IndexOf(methods, method.Name);
                    if (index > -1)
                    {
                        foundCount++;
                        filteredMethods[index] = method;
                    }
                }
            }

            if (foundCount != methods.Length)
            {
                var sb = new StringBuilder("Could not find method(s): ");
                for (int i = 0; i < filteredMethods.Length; i++)
                {
                    if (filteredMethods[i] == null)
                    {
                        sb.Append(methods[i]);
                        sb.Append(',');
                    }
                }

                sb.Length--;
                throw new XmlConfigException(sb + " on type " + obj.GetType() + Environment.NewLine + XmlHelper.GetXpath(node) + Environment.NewLine + node.OuterXml);
            }

            if (!forConfigCheckOnly)
            {
                foreach (MethodInfo method in filteredMethods)
                {
                    method.Invoke(obj, null);
                }
            }
        }

        private static void populateComponentStatePrivate(object o, XmlNode node, IObjectInstantiator instantiator, bool forConfigCheckOnly)
        {
            Dictionary<string, PropertyInfo> propertiesDictionary = buildPropertiesDictionary(o.GetType());
            Dictionary<string, XmlAttribute> attributesDictionary = buildAttributesDictionary(node.Attributes);

            setAttributeProperties(attributesDictionary, propertiesDictionary, o, node, false);
            setNodeProperties(propertiesDictionary, o, node, instantiator, forConfigCheckOnly);
            performCheckAfterComponentConfig(attributesDictionary, o, node, forConfigCheckOnly);
        }

        public static void SetComponentPropertiesFromAttributes(object o, XmlNode node, 
            IObjectInstantiator instantiator)
        {
            Dictionary<string, PropertyInfo> propertiesDictionary = buildPropertiesDictionary(o.GetType());
            Dictionary<string, XmlAttribute> attributesDictionary = buildAttributesDictionary(node.Attributes);

            setAttributeProperties(attributesDictionary, propertiesDictionary, o, node, false);
        }

        private static void populateListPrivate(IList list, Type genericParameter, XmlNode listNode, IObjectInstantiator instantiator, bool forConfigCheckOnly)
        {
            XmlNodeList nodes = XmlHelper.GetNodes(listNode, "Item");
            foreach (XmlNode listItemNode in XmlHelper.GetUncommentedChildNodes(nodes))
            {
                object listItem = buildComponent(listItemNode, instantiator, forConfigCheckOnly);
                if (listItem == null)
                {
                    ReflectionHelper.TryConvertToValueTypeOrStringOrType(genericParameter, listItemNode.InnerXml, out listItem);
                }

                if (listItem == null)
                {
                    listItem = instantiate(genericParameter, instantiator); 
                    if (listItem == null)
                    {
                        throw new XmlConfigException("Could not create list item of type " + genericParameter + " for the node " + XmlHelper.GetXpath(listItemNode) + Environment.NewLine + listItemNode.OuterXml);
                    }

                    populateComponentStatePrivate(listItem, listItemNode, instantiator, forConfigCheckOnly);
                }

                list.Add(listItem);
            }
        }

        private static void setAttributeProperties(IDictionary<string, XmlAttribute> attributesDictionary, IDictionary<string, PropertyInfo> propertiesDictionary, object obj, XmlNode node, bool isUpdate)
        {
            Type type = obj.GetType();
            var attributesToRemove = new List<string>();
            foreach (var kvp in attributesDictionary)
            {
                PropertyInfo property;
                if (propertiesDictionary.TryGetValue(kvp.Key, out property))
                {
                    if (!property.CanWrite)
                    {
                        throw new XmlConfigException("There property " + kvp.Key + " of the type " + type + " is readonly");
                    }

                    object convertedValue;
                    string attributeValue = kvp.Value.Value;
                    XmlHelper.ApplyAppKeyIfRequired(ref attributeValue);
                    if (ReflectionHelper.TryConvertToValueTypeOrStringOrType(property.PropertyType, attributeValue, out convertedValue))
                    {
                        if (isUpdate)
                        {
                            if (property.GetValue(obj, null) != convertedValue)
                            {
                                property.SetValue(obj, convertedValue, null);
                            }
                        }
                        else
                        {
                            property.SetValue(obj, convertedValue, null);
                        }

                        attributesToRemove.Add(kvp.Key);
                        propertiesDictionary.Remove(kvp.Key);
                    }
                    else
                    {
                        throw new XmlConfigException("Could not parse the property " + kvp.Key + " of the type " + type + " with the given string \"" + kvp.Value.Value + "\"");
                    }
                }
            }

            foreach (string s in attributesToRemove)
            {
                attributesDictionary.Remove(s);
            }
        }

        private static void setNodeProperties(IDictionary<string, PropertyInfo> propertiesDictionary, object obj, XmlNode node, IObjectInstantiator instantiator, bool forConfigCheckOnly)
        {
            Type type = obj.GetType();
            foreach (XmlNode childNode in XmlHelper.GetUncommentedChildNodes(node.ChildNodes))
            {
                PropertyInfo property;
                if (!propertiesDictionary.TryGetValue(childNode.Name, out property))
                {
                    throw new XmlConfigException("There is no property with the name " + childNode.Name + " in the type " + type + ". The node is: " + childNode.OuterXml);
                }

                if (!property.CanWrite)
                {
                    throw new XmlConfigException("There property " + childNode.Name + " of the type " + type + " is read-only");
                }

                object o = buildComponent(childNode, property, instantiator, forConfigCheckOnly);
                property.SetValue(obj, o, null);
                propertiesDictionary.Remove(property.Name);
            }
        }

        private static void updateList(PropertyInfo property, IList list, Type genericParameter, XmlNode listNode, IObjectInstantiator instantiator)
        {
            var dictionary = new Dictionary<object, int>();
            int i = 0;

            for (; i < list.Count; i++)
            {
                if (!genericParameter.IsValueType && genericParameter != typeof(string))
                {
                    PropertyInfo p = list[i].GetType().GetProperty("CategoryName");
                    if (p != null)
                    {
                        var name = p.GetValue(list[i], null) as string;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            dictionary.Add(name, i);
                        }
                    }
                }
            }

            i = 0;
            XmlNodeList nodes = XmlHelper.GetNodes(listNode, "Item");
            var newList = new object[nodes.Count];
            foreach (XmlNode listItemNode in XmlHelper.GetUncommentedChildNodes(nodes))
            {
                object value;
                bool isNonComplexType = ReflectionHelper.TryConvertToValueTypeOrStringOrType(genericParameter, listItemNode.InnerXml, out value);
                if (!isNonComplexType)
                {
                    value = XmlHelper.GetStringAttribute(listItemNode, "CategoryName");
                }

                int index;
                if (value != null && dictionary.TryGetValue(value, out index))
                {
                    object listItem = list[index];
                    newList[i] = listItem;
                    if (!genericParameter.IsValueType && genericParameter != typeof(string))
                    {
                        UpdateComponent(listItem, listItemNode, instantiator);
                    }
                }
                else
                {
                    object listItem = isNonComplexType ? value : BuildComponent(listItemNode, instantiator);
                    newList[i] = listItem;
                }

                i++;
            }

            for (i = 0; i < newList.Length; i++)
            {
                if (i < list.Count)
                {
                    list[i] = newList[i];
                }
                else
                {
                    list.Add(newList[i]);
                }
            }
        }

        private static void updateNodeProperties(IDictionary<string, PropertyInfo> propertiesDictionary, object obj, XmlNode node, IObjectInstantiator instantiator)
        {
            Type type = obj.GetType();
            foreach (XmlNode childNode in XmlHelper.GetUncommentedChildNodes(node.ChildNodes))
            {
                PropertyInfo property;
                if (!propertiesDictionary.TryGetValue(childNode.Name, out property))
                {
                    throw new XmlConfigException("There is no property with the name " + childNode.Name + " in the type " + type + ". The node is: " + childNode.OuterXml);
                }

                if (!property.CanWrite)
                {
                    throw new XmlConfigException("There property " + childNode.Name + " of the type " + type + " is readonly");
                }

                object o = property.GetValue(obj, null);
                if (o == null)
                {
                    o = BuildComponent(childNode, instantiator);
                    property.SetValue(obj, o, null);
                }
                else
                {
                    Type listType;
                    Type[] genericParameters;
                    bool isList = ReflectionHelper.CreateListIfPropertyTypeIsACollection(property.PropertyType, out listType, out genericParameters);
                    if (!isList)
                    {
                        UpdateComponent(o, childNode, instantiator);
                    }
                    else
                    {
                        var list = o as IList;
                        updateList(property, list, genericParameters[0], childNode, instantiator);
                    }
                }

                propertiesDictionary.Remove(property.Name);
            }
        }

        #endregion Methods
    }
}
