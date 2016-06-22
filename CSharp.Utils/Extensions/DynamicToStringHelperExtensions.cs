using System;
using System.Reflection;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Extensions
{
    public static class DynamicToStringHelperExtensions
    {
        #region Public Methods and Operators

        public static string ExportAsString(this object o)
        {
            if (o != null)
            {
                Type t = typeof(DynamicToStringHelper<>).MakeGenericType(new[] { o.GetType() });
                return t.InvokeMember("ExportAsString", BindingFlags.InvokeMethod, null, null, new[] { o }) as string;
            }

            return null;
        }

        public static string ExportAsString<T>(this T obj)
        {
            return DynamicToStringHelper<T>.ExportAsString(obj);
        }

        public static string ExportTypeUnknownObjectAsString(this object obj, string format)
        {
            if (obj != null)
            {
                Type t = typeof(DynamicToStringHelper<>).MakeGenericType(new[] { obj.GetType() });
                return t.InvokeMember("ExportAsString", BindingFlags.InvokeMethod, null, null, new[] { obj, format }) as string;
            }

            return null;
        }

        #endregion Public Methods and Operators
    }
}
