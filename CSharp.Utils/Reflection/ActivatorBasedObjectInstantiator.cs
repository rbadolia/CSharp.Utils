using System;

namespace CSharp.Utils.Reflection
{
    public sealed class ActivatorBasedObjectInstantiator : IObjectInstantiator
    {
        private static ActivatorBasedObjectInstantiator _instance = new ActivatorBasedObjectInstantiator();

        public static ActivatorBasedObjectInstantiator Instance
        {
            get { return _instance; }
        }

        private ActivatorBasedObjectInstantiator()
        {

        }

        public object Instantiate(Type objectType)
        {
            bool isSingleton;
            return ReflectionHelper.CreateOrGetObject<object>(objectType, out isSingleton);
        }
    }
}
