using System;
using System.Runtime.Serialization;
using CSharp.Utils.Contracts;
using CSharp.Utils.Reflection;
using CSharp.Utils.Runtime.Serialization.Formatters.Binary;

namespace CSharp.Utils
{
    [Serializable]
    [DataContract]
    public abstract class AbstractDeepCloneable<T> : IDeepCloneable<T> where T:class
    {
        public virtual T DeepClone()
        {
            return (T)BinaryFormatterHelper.DeepClone(this);
        }

        public override string ToString()
        {
            return DynamicToStringHelper<T>.ExportAsString(this as T);
        }
    }
}
