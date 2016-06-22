using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection
{
    public static class DynamicStringReplaceHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicStringReplaceHelper()
        {
            _methodDelegate = buildDynamicMethod();
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(T obj, string oldValue, string newValue, bool ignoreCase);

        #endregion Delegates

        #region Public Methods and Operators

        public static void ReplaceStrings(T obj, string oldValue, string newValue, bool ignoreCase = false)
        {
            _methodDelegate(obj, oldValue, newValue, ignoreCase);
        }

        public static void ReplaceStrings(IEnumerable<T> enumerable, string oldValue, string newValue, bool ignoreCase = false)
        {
            foreach (T obj in enumerable)
            {
                _methodDelegate(obj, oldValue, newValue, ignoreCase);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private static DynamicMethodDelegate buildDynamicMethod()
        {
            Type tType = typeof(T);
            MethodInfo replaceMethod = typeof(StringHelper).GetMethod("Replace", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string), typeof(string), typeof(bool) }, null);

            var dynamicMethod = new DynamicMethod("DynamicReplace", typeof(void), new[] { tType, typeof(string), typeof(string), typeof(bool) }, typeof(DynamicStringReplaceHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                {
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                    ilGen.Emit(OpCodes.Ldarg, 1);
                    ilGen.Emit(OpCodes.Ldarg, 2);
                    ilGen.Emit(OpCodes.Ldarg, 3);
                    ilGen.Emit(OpCodes.Call, replaceMethod);
                    ilGen.Emit(OpCodes.Callvirt, property.SetMethod);
                }
            }

            ilGen.Emit(OpCodes.Ret);
            var methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
            return methodDelegate;
        }

        #endregion Methods
    }
}
