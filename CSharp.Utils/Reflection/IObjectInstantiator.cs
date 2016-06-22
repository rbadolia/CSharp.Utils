using System;

namespace CSharp.Utils.Reflection
{
    public interface IObjectInstantiator
    {
        object Instantiate(Type objectType);
    }
}
