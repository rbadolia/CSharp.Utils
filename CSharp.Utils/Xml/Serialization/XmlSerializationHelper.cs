using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CSharp.Utils.Xml.Serialization
{
    public static class XmlSerializationHelper
    {
        #region Public Methods and Operators

        public static T Deserialize<T>(string xml, string root = null) where T : class
        {
            return Deserialize(typeof(T), xml, root) as T;
        }

        public static T Deserialize<T>(Stream stream, string root = null, XmlReaderSettings settings = null) where T : class
        {
            return Deserialize(typeof(T), stream, root, settings) as T;
        }

        public static T Deserialize<T>(TextReader reader, string root = null) where T : class
        {
            return Deserialize(typeof(T), reader, root) as T;
        }

        public static T Deserialize<T>(XmlReader reader, string root = null) where T : class
        {
            return Deserialize(typeof(T), reader, root) as T;
        }

        public static object Deserialize(Type objectType, string xml, string root = null)
        {
            using (var reader = new StringReader(xml))
            {
                return Deserialize(objectType, reader, root);
            }
        }

        public static object Deserialize(Type objectType, Stream stream, string root = null, XmlReaderSettings settings = null)
        {
            if (settings == null)
            {
                XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(objectType)) : (new XmlSerializer(objectType, new XmlRootAttribute(root)));
                return serializer.Deserialize(stream);
            }

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                return Deserialize(objectType, reader, root);
            }
        }

        public static object Deserialize(Type objectType, TextReader reader, string root = null)
        {
            XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(objectType)) : (new XmlSerializer(objectType, new XmlRootAttribute(root)));
            return serializer.Deserialize(reader);
        }

        public static object Deserialize(Type objectType, XmlReader reader, string root = null)
        {
            XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(objectType)) : (new XmlSerializer(objectType, new XmlRootAttribute(root)));
            return serializer.Deserialize(reader);
        }

        public static T DeserializeFromFile<T>(string fileName, string root = null, XmlReaderSettings settings = null) where T : class
        {
            return DeserializeFromFile(typeof(T), fileName, root, settings) as T;
        }

        public static object DeserializeFromFile(Type objectType, string fileName, string root = null, XmlReaderSettings settings = null)
        {
            using (FileStream reader = File.Open(fileName, FileMode.Open))
            {
                return Deserialize(objectType, reader, root, settings);
            }
        }

        public static string Serialize(object obj, string root = null)
        {
            using (var writer = new StringWriter())
            {
                Serialize(obj, writer, root);
                return writer.ToString();
            }
        }

        public static void Serialize(object obj, TextWriter writer, string root = null)
        {
            XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(obj.GetType())) : (new XmlSerializer(obj.GetType(), new XmlRootAttribute(root)));
            serializer.Serialize(writer, obj);
        }

        public static void Serialize(object obj, XmlWriter writer, string root = null)
        {
            XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(obj.GetType())) : (new XmlSerializer(obj.GetType(), new XmlRootAttribute(root)));
            serializer.Serialize(writer, obj);
        }

        public static void SerializeToFile(object obj, string fileName, string root = null)
        {
            using (var writer = new StreamWriter(fileName))
            {
                Serialize(obj, writer, root);
            }
        }

        public static void SerializeToStream(object obj, Stream stream, string root = null)
        {
            XmlSerializer serializer = string.IsNullOrWhiteSpace(root) ? (new XmlSerializer(obj.GetType())) : (new XmlSerializer(obj.GetType(), new XmlRootAttribute(root)));
            serializer.Serialize(stream, obj);
        }

        #endregion Public Methods and Operators
    }
}
