using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Reflection
{
    public static class DynamicComparerHelper<T>
    {
        private static readonly Dictionary<string, IComparer<T>> dictionary = new Dictionary<string, IComparer<T>>();

        static DynamicComparerHelper()
        {
            var parameterTypes = new[] { typeof(T), typeof(T) };

            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties<T>(false);
            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var dynamicMethod = new DynamicMethod("comparer_" + property.Name, typeof(int), parameterTypes, typeof(DynamicComparerHelper<T>));
                ILGenerator ilGen = dynamicMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                if (property.PropertyType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, property.PropertyType);
                }

                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                if (property.PropertyType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, property.PropertyType);
                }

                ilGen.Emit(OpCodes.Call, SharedReflectionInfo.CompareMethod);
                ilGen.Emit(OpCodes.Ret);

                var objectComparison = (Comparison<object>)dynamicMethod.CreateDelegate(typeof(Comparison<object>));
                var comparer = new DelegateBasedComparer<T>(delegate(T x, T y)
                    {
                        if (x != null && y != null)
                        {
                            return objectComparison(x, y);
                        }

                        if (x == null && y == null)
                        {
                            return 0;
                        }

                        return x == null ? 1 : -1;
                    });

                dictionary.Add(property.Name, comparer);
            }
        }

        public static IComparer<T> GetComparerForProperty(string propertyName)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(propertyName, propertyName);
            IComparer<T> comparer;
            dictionary.TryGetValue(propertyName, out comparer);
            return comparer;
        }
    }
}
