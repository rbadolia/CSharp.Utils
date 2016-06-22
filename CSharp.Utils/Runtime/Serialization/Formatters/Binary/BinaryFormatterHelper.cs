using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CSharp.Utils.Runtime.Serialization.Formatters.Binary
{
    public static class BinaryFormatterHelper
    {
        #region Public Methods and Operators

        public static object DeepClone(this object obj)
        {
            byte[] bytes = Serialize(obj);
            return Deserialize(bytes, bytes.LongLength);
        }

        public static object Deserialize(byte[] bytes, long length)
        {
            using (var memStream = new MemoryStream())
            {
                memStream.Write(bytes, 0, (int)length);
                memStream.Seek(0, SeekOrigin.Begin);
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memStream);
            }
        }

        public static object Deserialize(byte[] bytes)
        {
            return Deserialize(bytes, bytes.LongLength);
        }

        public static T Deserialize<T>(byte[] bytes, long length)
        {
            return (T)Deserialize(bytes, length);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(bytes, bytes.LongLength);
        }

        public static T Deserialize<T>(string fileName)
        {
            return (T)Deserialize(fileName);
        }

        public static object Deserialize(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(fs);
            }
        }

        public static void Serialize(object obj, string fileName)
        {
            using (FileStream fs = File.Create(fileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        public static byte[] Serialize(object obj)
        {
            using (var memStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memStream, obj);
                return memStream.ToArray();
            }
        }

        #endregion Public Methods and Operators
    }
}
