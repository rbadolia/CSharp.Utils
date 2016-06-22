using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.ComponentModel
{
    public static class DynamicNotifyPropertyChangeInjectionHelper<T>
    {
        private static volatile bool isInjected;

        private static readonly object SyncLock = new object();

        public static ReadOnlyCollection<PropertyChangedEventArgs> PropertyChangedEventArgsList
        {
            get; private set; 
        }

        public static ReadOnlyCollection<PropertyChangingEventArgs> PropertyChangingEventArgsList
        {
            get; private set; 
        }

        static DynamicNotifyPropertyChangeInjectionHelper()
        {
            IList<PropertyInfo> properties = ReflectionHelper.GetPublicProperties(typeof(T), false);
            List<PropertyChangedEventArgs> changedEventArgsList = null;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
            {
                changedEventArgsList = new List<PropertyChangedEventArgs>();
            }

            List<PropertyChangingEventArgs> changingEventArgsList = null;
            if (typeof(INotifyPropertyChanging).IsAssignableFrom(typeof(T)))
            {
                changingEventArgsList = new List<PropertyChangingEventArgs>();
            }

            if (changingEventArgsList == null && changedEventArgsList == null)
            {
                return;
            }

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    if (changedEventArgsList != null)
                    {
                        changedEventArgsList.Add(new PropertyChangedEventArgs(property.Name));
                    }

                    if (changingEventArgsList != null)
                    {
                        changingEventArgsList.Add(new PropertyChangingEventArgs(property.Name));
                    }
                }
            }

            if (changedEventArgsList != null)
            {
                PropertyChangedEventArgsList = changedEventArgsList.AsReadOnly();
            }

            if (changingEventArgsList != null)
            {
                PropertyChangingEventArgsList = changingEventArgsList.AsReadOnly();
            }
        }

        public static void Inject()
        {
            if (!isInjected)
            {
                lock (SyncLock)
                {
                    if (!isInjected)
                    {
                        isInjected = true;
                        InjectPropertyChange();
                    }
                }
            }
        }

        private static bool InjectPropertyChange()
        {
            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties(typeof(T), false);
            var propertyChangedEventField = typeof(T).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            var propertyChangingEventField = typeof(T).GetField("PropertyChanging", BindingFlags.Instance | BindingFlags.NonPublic);
            bool injectForPropertyChanged = PropertyChangedEventArgsList != null && propertyChangedEventField != null;
            bool injectForPropertyChanging = PropertyChangingEventArgsList != null && propertyChangingEventField != null;

            if (!injectForPropertyChanged && !injectForPropertyChanging)
            {
                return false;
            }

            var changingEventArgsPropertyGetterMethod = typeof(DynamicNotifyPropertyChangeInjectionHelper<T>).GetProperty("PropertyChangingEventArgsList", BindingFlags.Public | BindingFlags.Static).GetMethod;
            var changedEventArgsPropertyGetterMethod = typeof(DynamicNotifyPropertyChangeInjectionHelper<T>).GetProperty("PropertyChangedEventArgsList", BindingFlags.Public | BindingFlags.Static).GetMethod;

            var changingEventArgsIndexerGetterMethod = typeof(ReadOnlyCollection<PropertyChangingEventArgs>).GetProperties().First(p => p.GetIndexParameters().Length == 1).GetMethod;
            var changedEventArgsIndexerGetterMethod = typeof(ReadOnlyCollection<PropertyChangedEventArgs>).GetProperties().First(p => p.GetIndexParameters().Length == 1).GetMethod;

            int propertyIndex = -1;
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    propertyIndex++;
                    var setMethod = property.SetMethod;
                    var dynamicMethod = new DynamicMethod("NotifyPropertyChange_" + property.Name, setMethod.ReturnType, new[] { typeof(T), property.PropertyType }, typeof(T));

                    var ilGen = dynamicMethod.GetILGenerator();
                    ilGen.DeclareLocal(typeof(object[]));
                    if (injectForPropertyChanging)
                    {
                        ilGen.DeclareLocal(typeof(PropertyChangingEventHandler));
                    }

                    if (injectForPropertyChanged)
                    {
                        ilGen.DeclareLocal(typeof(PropertyChangedEventHandler));
                    }

                    if (injectForPropertyChanging)
                    {
                        InjectRaiseEvent(ilGen, propertyIndex, propertyChangingEventField, 1, changingEventArgsPropertyGetterMethod, changingEventArgsIndexerGetterMethod);
                    }

                    InjectCallActualMethod(ilGen, setMethod);

                    if (injectForPropertyChanged)
                    {
                        InjectRaiseEvent(ilGen, propertyIndex, propertyChangedEventField, injectForPropertyChanging ? 2 : 1, changedEventArgsPropertyGetterMethod, changedEventArgsIndexerGetterMethod);
                    }

                    ilGen.Emit(OpCodes.Ret);
                    try
                    {
                        MethodUtil.ReplaceMethod(dynamicMethod, setMethod);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                }
            }

            return true;
        }

        private static void InjectCallActualMethod(ILGenerator ilGen, MethodInfo setMethod)
        {
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Callvirt, setMethod);
        }

        private static void InjectRaiseEvent(ILGenerator ilGen, int propertyIndex, FieldInfo eventField, int eventFieldLocalIndex, MethodInfo eventArgsPropertyGetterMethod, MethodInfo indexerGetterMethod)
        {
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, eventField);
            ilGen.Emit(OpCodes.Stloc, eventFieldLocalIndex);

            ilGen.CreateArray(typeof(object), 2, 0);

            ilGen.ReplaceArrayElement(OpCodes.Ldloc_0, 0, OpCodes.Ldarg_0);
            int index = propertyIndex;
            ilGen.ReplaceArrayElement(OpCodes.Ldloc_0, 1, () =>
                {
                    ilGen.Emit(OpCodes.Call, eventArgsPropertyGetterMethod);
                    ilGen.Emit(OpCodes.Ldc_I4, index);
                    ilGen.Emit(OpCodes.Callvirt, indexerGetterMethod);
                });

            ilGen.Emit(OpCodes.Ldloc, eventFieldLocalIndex);
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Call, SharedReflectionInfo.SafeInvokeMethod);
        }
    }
}
