using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class DynamicIntelligentCopyHelper<TFrom, TTo>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicIntelligentCopyHelper()
        {
            Type sourceObjectType = typeof(TFrom);
            Type targetObjectType = typeof(TTo);

            var dynamicMethod = new DynamicMethod("Copy", typeof(void), new[] { sourceObjectType, targetObjectType }, typeof(DynamicIntelligentCopyHelper<TFrom, TTo>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            ilGen.DeclareLocal(typeof(int));

            PropertyInfo[] sourceProperties = sourceObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] targetProperties = targetObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            Label? returnLabel;
            Label? nextLabel;

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

                            if (sourceProperty.PropertyType == typeof(string))
                            {
                                MethodInfo mi = targetProperty.PropertyType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
                                if (mi != null)
                                {
                                    ilGen.Emit(OpCodes.Ldarg_1);
                                    ilGen.Emit(OpCodes.Ldarg_0);

                                    ilGen.Emit(OpCodes.Callvirt, sourceProperty.GetMethod);
                                    ilGen.Emit(OpCodes.Call, mi);
                                    ilGen.Emit(OpCodes.Stloc_0);
                                    ilGen.Emit(OpCodes.Ldloc_0);
                                    ilGen.Emit(OpCodes.Callvirt, targetProperty.SetMethod);
                                }

                                break;
                            }

                            if (!sourceProperty.PropertyType.IsValueType && !targetProperty.PropertyType.IsValueType && !targetProperty.PropertyType.IsAbstract)
                            {
                                ConstructorInfo constructor = targetProperty.PropertyType.GetConstructor(Type.EmptyTypes);
                                if (constructor != null)
                                {
                                    ilGen.Emit(OpCodes.Ldarg_0);
                                    ilGen.Emit(OpCodes.Callvirt, sourceProperty.GetMethod);
                                    ilGen.Emit(OpCodes.Ldnull);

                                    ilGen.Emit(OpCodes.Ceq);
                                    nextLabel = ilGen.DefineLabel();
                                    ilGen.Emit(OpCodes.Brfalse, nextLabel.Value);

                                    returnLabel = ilGen.DefineLabel();
                                    ilGen.Emit(OpCodes.Br, returnLabel.Value);

                                    ilGen.MarkLabel(nextLabel.Value);

                                    ilGen.Emit(OpCodes.Ldarg_1);
                                    ilGen.Emit(OpCodes.Newobj, constructor);
                                    ilGen.Emit(OpCodes.Callvirt, targetProperty.SetMethod);

                                    KeyValuePair<Type, Type>? kvp = IntelligentCopyHelper.CheckListTypes(sourceProperty.PropertyType, targetProperty.PropertyType);
                                    MethodInfo copyMethod;
                                    if (kvp != null)
                                    {
                                        Type genType = typeof(ListDynamicIntelligentCopyHelper<,>).MakeGenericType(new[] { kvp.Value.Key, kvp.Value.Value });
                                        copyMethod = genType.GetMethod("Copy", BindingFlags.Static | BindingFlags.Public);
                                    }
                                    else
                                    {
                                        Type genType = typeof(DynamicIntelligentCopyHelper<,>).MakeGenericType(new[] { sourceProperty.PropertyType, targetProperty.PropertyType });
                                        copyMethod = genType.GetMethod("Copy", BindingFlags.Static | BindingFlags.Public);
                                    }

                                    ilGen.Emit(OpCodes.Ldarg_0);
                                    ilGen.Emit(OpCodes.Callvirt, sourceProperty.GetMethod);
                                    ilGen.Emit(OpCodes.Ldarg_1);
                                    ilGen.Emit(OpCodes.Callvirt, targetProperty.GetMethod);
                                    ilGen.Emit(OpCodes.Call, copyMethod);

                                    ilGen.MarkLabel(returnLabel.Value);
                                }

                                break;
                            }
                        }
                    }
                }
            }

            ilGen.Emit(OpCodes.Ret);
            _methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(TFrom source, TTo target);

        #endregion Delegates

        #region Public Methods and Operators

        [CautionUsedByReflection]
        public static void Copy(TFrom source, TTo target)
        {
            _methodDelegate(source, target);
        }

        #endregion Public Methods and Operators
    }
}
