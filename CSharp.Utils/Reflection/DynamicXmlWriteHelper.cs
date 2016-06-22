using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Xml;
using CSharp.Utils.Reflection.Internal;
using CSharp.Utils.Xml;

namespace CSharp.Utils.Reflection
{
    public static class DynamicXmlWriteHelper<T>
    {
        #region Static Fields

        private static readonly WriteDynamicMethodDelegate _writeMethodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicXmlWriteHelper()
        {
            Type tType = typeof(T);
            MethodInfo writeAttributeMethodInfo = typeof(XmlHelper).GetMethod("WriteAttribute", BindingFlags.Public | BindingFlags.Static);

            var dynamicMethod = new DynamicMethod("DynamicXmlWrite", typeof(void), new[] { tType, typeof(XmlWriter) }, typeof(DynamicXmlWriteHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
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

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.PropertyType == typeof(string) || property.PropertyType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Ldstr, property.Name);

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                    if (property.PropertyType.IsEnum)
                    {
                        ilGen.Emit(OpCodes.Box, property.PropertyType);
                        ilGen.Emit(OpCodes.Call, SharedReflectionInfo.ObjectToStringMethod);
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

                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Call, writeAttributeMethodInfo);
                }
            }

            ilGen.Emit(OpCodes.Ret);
            _writeMethodDelegate = (WriteDynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(WriteDynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void WriteDynamicMethodDelegate(T obj, XmlWriter writer);

        #endregion Delegates

        #region Public Methods and Operators

        public static void Write(T obj, XmlWriter writer)
        {
            _writeMethodDelegate(obj, writer);
        }

        #endregion Public Methods and Operators
    }
}
