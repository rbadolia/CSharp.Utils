using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSharp.Utils.Reflection
{
    public interface IFilterTypesStrategy
    {
        IEnumerable<Type> FilterTypes(Assembly assembly);
    }
}
