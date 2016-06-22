using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CSharp.Utils.Data.Helpers;
using CSharp.Utils.Diagnostics.Performance;

namespace CSharp.Utils.Reflection.Internal
{
    internal class SharedReflectionInfo
    {
        #region Static Fields

        public static readonly MethodInfo SafeInvokeMethod = typeof(GeneralHelper).GetMethod("SafeInvoke", BindingFlags.Public | BindingFlags.Static);

        internal static readonly string[] Times = { "Ticks", "Milliseconds", "Seconds", "Minutes", "Hours" };

        internal static readonly MethodInfo ArrayIndexerGetMethod = typeof(object[]).GetMethod("Get");

        internal static readonly MethodInfo ArrayIndexerSetMethod = typeof(object[]).GetMethod("Set");

        internal static Dictionary<Type, MethodInfo> ConvertToMethods = new Dictionary<Type, MethodInfo>();

        internal static readonly MethodInfo GetValueFromDataRecordMethod = typeof(DataHelper).GetMethod("GetValueFromDataRecord", BindingFlags.Public | BindingFlags.Static);

        internal static readonly MethodInfo PerfCounterListIndexerGetMethod = typeof(IList<PerfCounter>).GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);

        internal static MethodInfo ObjectToStringMethod = typeof(StringHelper).GetMethod("GetObjectAsString", BindingFlags.Static | BindingFlags.Public);

        internal static readonly MethodInfo PerfCounterSetRawValueMethod = typeof(PerfCounter).GetMethod("set_RawValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(double) }, null);

        internal static readonly MethodInfo PerfCounterSetTagMethod = typeof(PerfCounter).GetMethod("set_Tag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object) }, null);

        internal static readonly MethodInfo RefreshCountersMethod = typeof(IRefreshCounters).GetMethod("RefreshCounters");

        internal static readonly Dictionary<Type, MethodInfo> TextWriterWriteMethods = new Dictionary<Type, MethodInfo>();

        internal static readonly MethodInfo[] TimeMethods = new MethodInfo[5];

        internal static readonly MethodInfo CompareMethod = typeof(GeneralHelper).GetMethod("Compare", BindingFlags.Static | BindingFlags.Public);

        static SharedReflectionInfo()
        {
            populateTimeMethods();
            populateConvertToMethods();
            populateTextWriterWriteMethods();
        }

        #endregion Constructors and Finalizers

        #region Methods

        private static void populateConvertToMethods()
        {
            Type t = typeof(Convert);
            MethodInfo[] methods = t.GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.IsStatic)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(String))
                    {
                        ConvertToMethods.Add(method.ReturnType, method);
                    }
                }
            }
        }

        private static void populateTextWriterWriteMethods()
        {
            MethodInfo[] methods = typeof(TextWriter).GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.ReturnType == typeof(void) && method.Name == "Write")
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        TextWriterWriteMethods.Add(parameters[0].ParameterType, method);
                    }
                }
            }
        }

        private static void populateTimeMethods()
        {
            TimeMethods[0] = typeof(TimeSpan).GetProperty("Ticks").GetMethod;
            for (int i = 1; i < 5; i++)
            {
                TimeMethods[i] = typeof(TimeSpan).GetProperty("Total" + Times[i]).GetMethod;
            }
        }

        #endregion Methods
    }
}
