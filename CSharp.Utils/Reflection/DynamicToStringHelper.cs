using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class DynamicToStringHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicToStringHelper()
        {
            _methodDelegate = buildDynamicMethod();
        }

        #endregion Constructors and Finalizers

        #region Delegates

        public delegate string GetPropertyValueDelegate(string propertyName, object propertyValue, bool isLastProperty);

        public delegate void WritePropertyValueDelegate(string propertyName, object propertyValue, TextWriter writer, bool isLastProperty);

        private delegate void DynamicMethodDelegate(T obj, WritePropertyValueDelegate del, TextWriter writer);

        #endregion Delegates

        #region Public Methods and Operators

        public static string ExportAsString(T obj, bool excludeNullValues = false, bool dontWritePropertyNames = false, string prefix = null, string separator = " = ", string suffix = ",")
        {
            using (var writer = new StringWriter())
            {
                _methodDelegate(obj, (propertyName, propertyValue, writer1, isLastProperty) => DynamicToStringHelperAppend.WriteToWriter(excludeNullValues, dontWritePropertyNames, prefix, separator, suffix, propertyName, propertyValue, writer1, isLastProperty), writer);
                return writer.ToString();
            }
        }

        public static string ExportAsString(T obj, GetPropertyValueDelegate del)
        {
            using (var writer = new StringWriter())
            {
                _methodDelegate(obj, delegate(string propertyName, object propertyValue, TextWriter writer1, bool isLastProperty)
                    {
                        string s = del(propertyName, propertyValue, isLastProperty);
                        if (s != null)
                        {
                            writer.Write(s);
                        }
                    }, writer);
                return writer.ToString();
            }
        }

        public static void WriteToString(T obj, TextWriter writer, bool excludeNullValues = false, bool dontWritePropertyNames = false, string prefix = null, string separator = " = ", string suffix = ",")
        {
            _methodDelegate(obj, (propertyName, propertyValue, writer1, isLastProperty) => DynamicToStringHelperAppend.WriteToWriter(excludeNullValues, dontWritePropertyNames, prefix, separator, suffix, propertyName, propertyValue, writer1, isLastProperty), writer);
        }

        public static void WriteToString(T obj, GetPropertyValueDelegate del, TextWriter writer)
        {
            _methodDelegate(obj, delegate(string propertyName, object propertyValue, TextWriter writer1, bool isLastProperty)
                {
                    string s = del(propertyName, propertyValue, isLastProperty);
                    if (s != null)
                    {
                        writer.Write(s);
                    }
                }, writer);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static DynamicMethodDelegate buildDynamicMethod()
        {
            Type objectType = typeof(T);
            MethodInfo invokeMethodInfo = typeof(WritePropertyValueDelegate).GetMethod("Invoke");
            MethodInfo objectToStringMethod = typeof(Object).GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

            var dynamicMethod = new DynamicMethod("DynamicString", typeof(void), new[] { objectType, typeof(WritePropertyValueDelegate), typeof(TextWriter) }, typeof(DynamicToStringHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, ColumnOrderComparer.Instance);
            var locals = new Dictionary<Type, int>();
            int localsIndex = 0;
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && !property.PropertyType.IsEnum && property.PropertyType.IsValueType)
                {
                    if (!locals.ContainsKey(property.PropertyType))
                    {
                        ilGen.DeclareLocal(property.PropertyType);
                        locals.Add(property.PropertyType, localsIndex);
                        localsIndex++;
                    }
                }
            }

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.CanRead)
                {
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldstr, property.Name);

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);

                    if (property.PropertyType.IsEnum)
                    {
                        ilGen.Emit(OpCodes.Box, property.PropertyType);
                        ilGen.Emit(OpCodes.Callvirt, objectToStringMethod);
                    }
                    else
                    {
                        if (property.PropertyType.IsValueType)
                        {
                            ilGen.Emit(OpCodes.Stloc, locals[property.PropertyType]);
                            ilGen.Emit(OpCodes.Ldloca, locals[property.PropertyType]);

                            MethodInfo toStringMethod = property.PropertyType.GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                            object[] securitySafeCriticalAttributes = toStringMethod.GetCustomAttributes(typeof(SecuritySafeCriticalAttribute), true);
                            if (securitySafeCriticalAttributes.Length > 0)
                            {
                                ilGen.Emit(OpCodes.Constrained, property.PropertyType);
                            }

                            ilGen.Emit(OpCodes.Callvirt, toStringMethod);
                        }
                    }

                    ilGen.Emit(OpCodes.Ldarg_2);
                    ilGen.Emit(OpCodes.Ldc_I4, i == properties.Length - 1 ? 1 : 0);

                    ilGen.Emit(OpCodes.Callvirt, invokeMethodInfo);
                }
            }

            ilGen.Emit(OpCodes.Ret);
            var methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
            return methodDelegate;
        }

        #endregion Methods
    }
}
