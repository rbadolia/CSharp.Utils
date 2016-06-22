using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Diagnostics.Performance
{
    public static class DynamicPerfCounterUpdater<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate MethodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicPerfCounterUpdater()
        {
            Type t = typeof(T);
            object[] categoryAttributes = t.GetCustomAttributes(typeof(PerfCounterCategoryAttribute), false);
            if (categoryAttributes.Length == 0)
            {
                return;
            }

            MetaData = new TypeMetaData { Type = t };
            var c = (PerfCounterCategoryAttribute)categoryAttributes[0];
            MetaData.CategoryName = c.CategoryName;
            MetaData.CategoryHelp = c.CategoryHelp;

            var dm = new DynamicMethod("Update", typeof(void), new[] { typeof(IList<PerfCounter>), t }, typeof(DynamicPerfCounterUpdater<T>));
            ILGenerator gen = dm.GetILGenerator();
            var list = new List<PropertyInfo>();

            List<KeyValuePair<PropertyInfo, PerfCounterAttribute>> properties = ReflectionHelper.GetMandatoryPropertyCustomAttributes<PerfCounterAttribute>(t, false);
            properties.Sort(CounterGroupNameComparer.Instance);
            bool hasTimeSpan = false;
            foreach (var kvp in properties)
            {
                if (kvp.Key.PropertyType == typeof(TimeSpan))
                {
                    hasTimeSpan = true;
                    break;
                }
            }

            if (hasTimeSpan)
            {
                gen.DeclareLocal(typeof(TimeSpan));
            }

            if (typeof(IRefreshCounters).IsAssignableFrom(t))
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.RefreshCountersMethod);
            }

            int i = 0;
            foreach (var kvp in properties)
            {
                PropertyInfo property = kvp.Key;
                if (property.CanRead)
                {
                    if (isNumericType(property.PropertyType))
                    {
                        if (kvp.Value.SizeType == SizeTypes.None)
                        {
                            handleValueType(gen, property, kvp.Value, ref i);
                        }
                        else
                        {
                            HandleSizeType(gen, property, kvp.Value, ref i);
                        }
                    }
                    else
                    {
                        if (property.PropertyType == typeof(TimeSpan))
                        {
                            handleTimeStampType(gen, property, kvp.Value, ref i);
                        }
                        else
                        {
                            handleReferenceType(gen, property, kvp.Value, ref i);
                        }
                    }
                }
            }

            gen.Emit(OpCodes.Ret);
            MethodDelegate = (DynamicMethodDelegate)dm.CreateDelegate(typeof(DynamicMethodDelegate));
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(IList<PerfCounter> counters, T obj);

        #endregion Delegates

        #region Public Properties

        public static TypeMetaData MetaData { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static void Update(IList<PerfCounter> counters, T obj)
        {
            MethodDelegate(counters, obj);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void handleReferenceType(ILGenerator gen, PropertyInfo property, PerfCounterAttribute att, ref int i)
        {
            PropertyMetaData pMetaData = PropertyMetaData.CreateNew(property.Name, att.GroupName, property.Name);
            MetaData.Properties.Add(pMetaData);

            pMetaData.IsValidForWindowsPerfCounter = false;

            injectGetCounterValueInstructions(gen, property, i);
            if (property.PropertyType.IsValueType)
            {
                gen.Emit(OpCodes.Box, property.PropertyType);
            }

            gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.PerfCounterSetTagMethod);
            i++;
        }

        private static void HandleSizeType(ILGenerator gen, PropertyInfo property, PerfCounterAttribute perfCounterAttribute, ref int i)
        {
            var j = (int)Math.Log((int)perfCounterAttribute.SizeType, 2);
            SizeTypes counterSizeType = perfCounterAttribute.CounterSizeTypes;
            if (perfCounterAttribute.CounterSizeTypes == 0)
            {
                counterSizeType = perfCounterAttribute.SizeType;
            }

            for (int k = 1; k <= 4; k++)
            {
                var pow = (int)Math.Pow(2, k);
                if (((int)counterSizeType & pow) != 0)
                {
                    PropertyMetaData pMetaData = PropertyMetaData.CreateNew(property.Name, perfCounterAttribute.GroupName, property.Name + "_In" + ((SizeTypes)pow));
                    MetaData.Properties.Add(pMetaData);

                    injectGetCounterValueInstructions(gen, property, i);

                    if (property.PropertyType != typeof(double))
                    {
                        gen.Emit(OpCodes.Conv_R8);
                    }

                    var divValue = (long)Math.Pow(1024, Math.Abs(j - k));
                    gen.Emit(OpCodes.Ldc_I8, divValue);
                    gen.Emit(k > j ? OpCodes.Div : OpCodes.Mul);
                    gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.PerfCounterSetRawValueMethod);
                    i++;
                }
            }
        }

        private static void handleTimeStampType(ILGenerator gen, PropertyInfo property, PerfCounterAttribute att, ref int i)
        {
            TimeTypes counterTimeTypes = att.CounterTimeTypes;
            if (counterTimeTypes == TimeTypes.None)
            {
                counterTimeTypes = TimeTypes.Ticks;
            }

            for (int j = 0; j < SharedReflectionInfo.TimeMethods.Length; j++)
            {
                var pow = (int)Math.Pow(2, j + 1);
                if (((int)counterTimeTypes & pow) != 0)
                {
                    PropertyMetaData pMetaData = PropertyMetaData.CreateNew(property.Name, att.GroupName, property.Name + "_In" + ((TimeTypes)pow));
                    MetaData.Properties.Add(pMetaData);

                    injectGetCounterValueInstructions(gen, property, i);
                    gen.Emit(OpCodes.Stloc_0);
                    gen.Emit(OpCodes.Ldloca_S, 0);

                    gen.Emit(OpCodes.Call, SharedReflectionInfo.TimeMethods[j]);
                    gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.PerfCounterSetRawValueMethod);
                    i++;
                }
            }
        }

        private static void handleValueType(ILGenerator gen, PropertyInfo property, PerfCounterAttribute att, ref int i)
        {
            PropertyMetaData pMetaData = PropertyMetaData.CreateNew(property.Name, att.GroupName, property.Name);
            MetaData.Properties.Add(pMetaData);

            injectGetCounterValueInstructions(gen, property, i);
            if (property.PropertyType != typeof(double))
            {
                gen.Emit(OpCodes.Conv_R8);
            }

            gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.PerfCounterSetRawValueMethod);
            i++;
        }

        private static void injectGetCounterValueInstructions(ILGenerator gen, PropertyInfo property, int index)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, index);
            gen.Emit(OpCodes.Callvirt, SharedReflectionInfo.PerfCounterListIndexerGetMethod);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, property.GetMethod);
        }

        private static bool isNumericType(Type t)
        {
            return t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(long) || t == typeof(float) || t == typeof(double);
        }

        #endregion Methods
    }
}
