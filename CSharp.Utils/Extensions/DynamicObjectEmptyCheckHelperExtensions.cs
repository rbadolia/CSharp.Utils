using System;
using System.Collections.Generic;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Extensions
{
    public static class DynamicObjectEmptyCheckHelperExtensions
    {
        #region Public Methods and Operators

        public static bool DynamicIsEmpty<T>(this T obj, bool trimStrings = false)
        {
            if (obj == null)
            {
                return true;
            }

            return DynamicObjectEmptyCheckHelper<T>.IsEmpty(obj, trimStrings);
        }

        public static bool DynamicIsEmpty(this object obj, bool trimStrings = false)
        {
            if (obj == null)
            {
                return true;
            }

            Type t = typeof(DynamicObjectEmptyCheckHelper<>).MakeGenericType(new[] { obj.GetType() });
            return (bool)t.InvokeMember("IsEmpty", BindingFlags.InvokeMethod, null, null, new[] { obj, trimStrings });
        }

        public static void DynamicRemoveEmptyElements<T>(this IList<T> list, bool trimStrings = false)
        {
            Type t = typeof(DynamicObjectEmptyCheckHelper<>).MakeGenericType(new[] { typeof(T) });
            t.InvokeMember("RemoveEmptyElements", BindingFlags.InvokeMethod, null, null, new object[] { list, trimStrings });
        }

        #endregion Public Methods and Operators
    }
}
