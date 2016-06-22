using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection
{
    public static class DynamicStringTrimHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicStringTrimHelper()
        {
            Type tType = typeof(T);
            MethodInfo trimMethod = typeof(StringHelper).GetMethod("Trim", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object) }, null);

            var dynamicMethod = new DynamicMethod("DynamicTrim", typeof(void), new[] { tType }, typeof(DynamicStringTrimHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                {
                    ilGen.Emit(OpCodes.Ldarg_0);

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);

                    ilGen.Emit(OpCodes.Call, trimMethod);

                    ilGen.Emit(OpCodes.Callvirt, property.SetMethod);
                }
            }

            ilGen.Emit(OpCodes.Ret);
            _methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(T obj);

        #endregion Delegates

        #region Public Methods and Operators

        public static void TrimStrings(T obj)
        {
            _methodDelegate(obj);
        }

        public static void TrimStrings(IEnumerable<T> enumerable)
        {
            foreach (T obj in enumerable)
            {
                TrimStrings(obj);
            }
        }

        #endregion Public Methods and Operators
    }
}
