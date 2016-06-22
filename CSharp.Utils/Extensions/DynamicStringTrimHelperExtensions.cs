using System;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Extensions
{
    public static class DynamicStringTrimHelperExtensions
    {
        #region Public Methods and Operators

        public static void DynamicTrimStrings(this object obj)
        {
            if (obj != null)
            {
                Type t = typeof(DynamicStringTrimHelper<>).MakeGenericType(new[] { obj.GetType() });
                t.InvokeMember("TrimStrings", BindingFlags.InvokeMethod, null, null, new[] { obj });
            }
        }

        public static void DynamicTrimStrings<T>(this T obj)
        {
            if (obj != null)
            {
                DynamicStringTrimHelper<T>.TrimStrings(obj);
            }
        }

        #endregion Public Methods and Operators
    }
}
