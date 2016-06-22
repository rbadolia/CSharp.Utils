using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class DynamicObjectEmptyCheckHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicObjectEmptyCheckHelper()
        {
            Type tType = typeof(T);
            var dynamicMethod = new DynamicMethod("DynamicEmptyCheck", typeof(bool), new[] { tType }, typeof(DynamicObjectEmptyCheckHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Label? nextLabel = null;

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    if (nextLabel != null)
                    {
                        ilGen.MarkLabel(nextLabel.Value);
                    }

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                    Type t = typeof(ValueTypeDefaultCheckHelper<>).MakeGenericType(property.PropertyType);
                    MethodInfo mi = t.GetMethod("IsDefault", BindingFlags.Static | BindingFlags.NonPublic);
                    ilGen.Emit(OpCodes.Call, mi);
                    ilGen.Emit(OpCodes.Ldc_I4_1);
                    ilGen.Emit(OpCodes.Ceq);
                    nextLabel = ilGen.DefineLabel();
                    ilGen.Emit(OpCodes.Brtrue, nextLabel.Value);

                    ilGen.Emit(OpCodes.Ldc_I4, 0);
                    ilGen.Emit(OpCodes.Ret);
                }
            }

            if (nextLabel != null)
            {
                ilGen.MarkLabel(nextLabel.Value);
            }

            ilGen.Emit(OpCodes.Ldc_I4, 1);
            ilGen.Emit(OpCodes.Ret);
            _methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate bool DynamicMethodDelegate(T obj);

        #endregion Delegates

        #region Public Methods and Operators

        public static bool IsEmpty(T obj, bool trimStrings = false)
        {
            if (trimStrings)
            {
                DynamicStringTrimHelper<T>.TrimStrings(obj);
            }

            if (obj == null)
            {
                return true;
            }

            return _methodDelegate(obj);
        }

        public static void RemoveEmptyElements(IList<T> list, bool trimStrings = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (IsEmpty(list[i], trimStrings))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
