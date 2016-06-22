using System;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class DynamicCopyHelper<TFrom, TTo>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicCopyHelper()
        {
            _methodDelegate = buildDynamicMethod();
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(TFrom source, TTo target);

        #endregion Delegates

        #region Public Methods and Operators

        public static void Copy(TFrom source, TTo target)
        {
            _methodDelegate(source, target);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static DynamicMethodDelegate buildDynamicMethod()
        {
            Type sourceObjectType = typeof(TFrom);
            Type targetObjectType = typeof(TTo);

            var dynamicMethod = new DynamicMethod("Copy", typeof(void), new[] { sourceObjectType, targetObjectType }, typeof(DynamicCopyHelper<TFrom, TTo>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] sourceProperties = sourceObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] targetProperties = targetObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                if (Attribute.IsDefined(sourceProperty, typeof(DoNotCopyAttribute)))
                {
                    continue;
                }

                if (sourceProperty.CanRead)
                {
                    foreach (PropertyInfo targetProperty in targetProperties)
                    {
                        if (targetProperty.CanWrite && sourceProperty.Name == targetProperty.Name)
                        {
                            if (sourceProperty.PropertyType == targetProperty.PropertyType)
                            {
                                ilGen.Emit(OpCodes.Ldarg_1);
                                ilGen.Emit(OpCodes.Ldarg_0);
                                ilGen.Emit(OpCodes.Callvirt, sourceProperty.GetMethod);
                                ilGen.Emit(OpCodes.Callvirt, targetProperty.SetMethod);
                                break;
                            }

                            if (sourceProperty.PropertyType.IsValueType && sourceProperty.PropertyType.IsGenericType && sourceProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && sourceProperty.PropertyType.GetGenericArguments()[0] == targetProperty.PropertyType)
                            {
                                ilGen.Emit(OpCodes.Ldarg_1);
                                ilGen.Emit(OpCodes.Ldarg_0);
                                ilGen.Emit(OpCodes.Callvirt, sourceProperty.GetMethod);

                                MethodInfo mi = typeof(IntelligentCopyHelper).GetMethod("GetNonNullableValue", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(targetProperty.PropertyType);
                                ilGen.Emit(OpCodes.Call, mi);
                                ilGen.Emit(OpCodes.Callvirt, targetProperty.SetMethod);
                                break;
                            }
                        }
                    }
                }
            }

            ilGen.Emit(OpCodes.Ret);
            var methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
            return methodDelegate;
        }

        #endregion Methods
    }
}
