using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Data.Helpers
{
    public static class ArrayHelper
    {
        public delegate void PopulateObjectFromArrayCallback<in T>(T obj, object[] array);

        public static PopulateObjectFromArrayCallback<T> BuildDynamicMethodForPopulatingObject<T>(IEnumerable<PropertyOrdinalInfo> propertyOrdinalInfos, bool isFromDataRecord)
        {
            var dynamicMethod = new DynamicMethod("populateObjectFromArray" + GeneralHelper.Identity, typeof(void), new[] { typeof(T), typeof(object[]) }, typeof(ArrayHelper));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            foreach (var propertyOrdinalInfo in propertyOrdinalInfos)
            {
                Type propertyType = propertyOrdinalInfo.Property.PropertyType;
                MethodInfo convertMethod = null;
                int convertMode = -1;
                Type nullableType = null;
                if (!propertyType.IsEnum)
                {
                    Type fieldType = propertyOrdinalInfo.InputDataType;
                    if (propertyType == fieldType || propertyType == typeof(object) || (fieldType.IsValueType && propertyType == typeof(Nullable<>).MakeGenericType(fieldType)))
                    {
                        convertMode = 0;
                    }

                    if (convertMode == -1 && propertyType.IsValueType)
                    {
                        Type t = propertyType;

                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            t = propertyType.GetGenericArguments()[0];
                            nullableType = t;
                        }

                        if (SharedReflectionInfo.ConvertToMethods.TryGetValue(t, out convertMethod))
                        {
                            convertMode = 1;
                        }
                    }

                    if (convertMode == -1)
                    {
                        MethodInfo mi = propertyType.GetMethod("Parse", new[] { typeof(string) });

                        if (mi != null && mi.IsStatic && mi.ReflectedType == propertyType)
                        {
                            ParameterInfo[] parameters = mi.GetParameters();
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                            {
                                convertMode = 2;
                                convertMethod = mi;
                            }
                        }
                    }
                }
                else
                {
                    convertMethod = typeof(ReflectionHelper).GetMethod("ParseEnum").MakeGenericMethod(propertyType);
                    convertMode = 3;
                }

                if (convertMode != -1)
                {
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldc_I4, propertyOrdinalInfo.Ordinal);
                    MethodInfo getValueConcreteMethod = isFromDataRecord ? SharedReflectionInfo.GetValueFromDataRecordMethod.MakeGenericMethod(convertMode == 0 ? propertyType : typeof(object)) : SharedReflectionInfo.ArrayIndexerGetMethod;
                    ilGen.Emit(OpCodes.Call, getValueConcreteMethod);
                    if (convertMode != 0)
                    {
                        ilGen.Emit(OpCodes.Call, SharedReflectionInfo.ObjectToStringMethod);
                        ilGen.Emit(OpCodes.Call, convertMethod);
                        if (convertMode == 3)
                        {
                            ilGen.Emit(OpCodes.Unbox_Any, propertyType);
                        }
                    }

                    if (nullableType != null)
                    {
                        ConstructorInfo c = typeof(Nullable<>).MakeGenericType(nullableType).GetConstructor(new[] { nullableType });
                        ilGen.Emit(OpCodes.Newobj, c);
                    }

                    ilGen.Emit(OpCodes.Callvirt, propertyOrdinalInfo.Property.SetMethod);
                }
            }

            ilGen.Emit(OpCodes.Ret);

            return (PopulateObjectFromArrayCallback<T>)dynamicMethod.CreateDelegate(typeof(PopulateObjectFromArrayCallback<T>));
        }
    }
}
