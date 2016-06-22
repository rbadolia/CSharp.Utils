using System;
using System.Collections.Generic;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public interface IFilterMethodsStrategy<T>
        where T : Attribute
    {
        #region Public Methods and Operators

        IEnumerable<InjectableMethodInfo<T>> FilterMethods(Assembly assembly);

        #endregion Public Methods and Operators
    }
}
