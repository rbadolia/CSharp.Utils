using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CSharp.Utils.Web
{
    public static class JsonSerializationHelper
    {
        #region Public Methods and Operators

        public static T Deserialize<T>(string content, Encoding encoding = null) where T : class
        {
            return Deserialize(typeof(T), content, encoding) as T;
        }

        public static T Deserialize<T>(Stream stream) where T : class
        {
            return Deserialize(typeof(T), stream) as T;
        }

        public static object Deserialize(Type objectType, string content, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var bytes = Encoding.UTF8.GetBytes(content);
            using(MemoryStream stream = new MemoryStream(bytes))
            {
                return Deserialize(objectType, stream);
            }
        }

        public static object Deserialize(Type objectType, Stream stream)
        {
            var serializer = new DataContractJsonSerializer(objectType);
            return serializer.ReadObject(stream);
        }

        public static T DeserializeFromFile<T>(string fileName, Encoding encoding = null) where T : class
        {
            return DeserializeFromFile(typeof(T), fileName, encoding) as T;
        }

        public static object DeserializeFromFile(Type objectType, string fileName, Encoding encoding = null)
        {
            using (FileStream reader = File.Open(fileName, FileMode.Open))
            {
                return Deserialize(objectType, reader);
            }
        }

        public static string Serialize(object obj, Encoding encoding = null)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SerializeToStream(obj, stream);
                encoding = encoding ?? Encoding.UTF8;

                string jsonString = encoding.GetString(stream.ToArray());
                return jsonString;
            }
        }

        public static void SerializeToFile(object obj, string fileName, Encoding encoding = null)
        {
            using (var writer = encoding == null ? new StreamWriter(fileName) : new StreamWriter(fileName, false, encoding))
            {
                SerializeToStream(obj, writer.BaseStream);
            }
        }

        public static void SerializeToStream(object obj, Stream stream)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            serializer.WriteObject(stream, obj);
        }

        #endregion Public Methods and Operators
    }
}
