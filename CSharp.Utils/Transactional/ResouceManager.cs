using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Transactions;

namespace CSharp.Utils.Transactional
{
    public static class ResourceManager
    {
        public static T Clone<T>(T source)
        {
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            Debug.Assert(typeof(T).IsSerializable);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                T clone = (T)formatter.Deserialize(stream);
                return clone;
            }
        }

        public static void ConstrainType(Type type)
        {
            bool serializable = type.IsSerializable;
            if (serializable == false)
            {
                string message = "The type " + type + " is not serializable";
                throw new InvalidOperationException(message);
            }
        }

        public static bool SameTransaction(TransactionInformation transaction1, TransactionInformation transaction2)
        {
            if (transaction1.DistributedIdentifier == Guid.Empty && transaction1.DistributedIdentifier == Guid.Empty)
            {
                return transaction1.LocalIdentifier == transaction2.LocalIdentifier;
            }

            return transaction1.DistributedIdentifier == transaction2.DistributedIdentifier;
        }
    }
}
