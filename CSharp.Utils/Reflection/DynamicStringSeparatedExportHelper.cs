using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class DynamicStringSeparatedExportHelper<T>
    {
        #region Static Fields

        private static readonly HeaderDynamicMethodDelegate _headerMethodDelegate;

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicStringSeparatedExportHelper()
        {
            Type tType = typeof(T);

            var dynamicMethod = new DynamicMethod("DynamicStringSeparatedExport", typeof(void), new[] { tType, typeof(TextWriter), typeof(string) }, typeof(DynamicStringSeparatedExportHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            var headerDynamicMethod = new DynamicMethod("DynamicCharSeparatedExportHeader", typeof(void), new[] { typeof(TextWriter), typeof(string) }, typeof(DynamicStringSeparatedExportHelper<T>));
            ILGenerator headerIlGen = headerDynamicMethod.GetILGenerator();

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, ColumnOrderComparer.Instance);
            MethodInfo writeStringMethodInfo = SharedReflectionInfo.TextWriterWriteMethods[typeof(string)];
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.CanRead)
                {
                    bool boxingRequired = false;

                    MethodInfo textWriterMethod;
                    if (!SharedReflectionInfo.TextWriterWriteMethods.TryGetValue(property.PropertyType, out textWriterMethod))
                    {
                        if (SharedReflectionInfo.TextWriterWriteMethods.TryGetValue(typeof(object), out textWriterMethod))
                        {
                            if (property.PropertyType.IsValueType)
                            {
                                boxingRequired = true;
                            }
                        }
                    }

                    if (textWriterMethod != null)
                    {
                        headerIlGen.Emit(OpCodes.Ldarg, 0);
                        headerIlGen.Emit(OpCodes.Ldstr, property.Name);
                        headerIlGen.Emit(OpCodes.Callvirt, writeStringMethodInfo);

                        ilGen.Emit(OpCodes.Ldarg, 1);

                        ilGen.Emit(OpCodes.Ldarg, 0);
                        ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                        if (boxingRequired)
                        {
                            ilGen.Emit(OpCodes.Box, property.PropertyType);
                        }

                        ilGen.Emit(OpCodes.Callvirt, textWriterMethod);
                        if (i != properties.Length - 1)
                        {
                            headerIlGen.Emit(OpCodes.Ldarg, 0);
                            headerIlGen.Emit(OpCodes.Ldarg, 1);
                            headerIlGen.Emit(OpCodes.Callvirt, writeStringMethodInfo);

                            ilGen.Emit(OpCodes.Ldarg, 1);
                            ilGen.Emit(OpCodes.Ldarg, 2);
                            ilGen.Emit(OpCodes.Callvirt, writeStringMethodInfo);
                        }
                    }
                }
            }

            headerIlGen.Emit(OpCodes.Ret);
            ilGen.Emit(OpCodes.Ret);

            _methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
            _headerMethodDelegate = (HeaderDynamicMethodDelegate)headerDynamicMethod.CreateDelegate(typeof(HeaderDynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(T obj, TextWriter writer, string separator);

        private delegate void HeaderDynamicMethodDelegate(TextWriter writer, string separator);

        #endregion Delegates

        #region Public Methods and Operators

        public static void WriteHeaderToTextWriter(TextWriter writer, string separator = "\t")
        {
            _headerMethodDelegate(writer, separator);
        }

        public static void WriteToTextWriter(T obj, TextWriter writer, string separator = "\t")
        {
            _methodDelegate(obj, writer, separator);
        }

        public static void WriteToTextWriter(IEnumerable<T> enumerable, TextWriter writer, string separator = "\t", bool writeHeader = false)
        {
            if (writeHeader)
            {
                WriteHeaderToTextWriter(writer, separator);
                writer.Write(Environment.NewLine);
            }

            foreach (T v in enumerable)
            {
                WriteToTextWriter(v, writer, separator);
                writer.Write(Environment.NewLine);
            }
        }

        #endregion Public Methods and Operators
    }
}
