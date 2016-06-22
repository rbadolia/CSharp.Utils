using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Data.Helpers
{
    public static class DynamicToDataSetHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicToDataSetHelper()
        {
            _methodDelegate = BuildDynamicMethod();
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(T obj, DataSet dataSet);

        #endregion Delegates

        #region Public Methods and Operators

        public static DataSet ExportAsDataSet(T obj)
        {
            var dataSet = new DataSet();
            _methodDelegate(obj, dataSet);
            return dataSet;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static DynamicMethodDelegate BuildDynamicMethod()
        {
            Type objectType = typeof(T);
            var dynamicMethod = new DynamicMethod("ExportAsDataSet", typeof(void), new[] { objectType, typeof(DataSet) }, typeof(DynamicToDataSetHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            ilGen.DeclareLocal(typeof(int));

            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties(objectType, true);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && !property.IsDefined(typeof(NotDataTableExportableAttribute), false))
                {
                    if (property.PropertyType.IsGenericType)
                    {
                        Type acceptableType = typeof(IEnumerable<>);
                        Type[] generics = property.PropertyType.GetGenericArguments();
                        if (generics.Length == 1)
                        {
                            Type genericType = acceptableType.MakeGenericType(generics);
                            if (genericType.IsAssignableFrom(property.PropertyType))
                            {
                                MethodInfo method = typeof(DynamicToDataSetHelperExtensions).GetMethod("AddToDataSet").MakeGenericMethod(generics);
                                ilGen.Emit(OpCodes.Ldarg_0);
                                ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                                ilGen.Emit(OpCodes.Ldarg_1);
                                ilGen.Emit(OpCodes.Ldstr, property.Name);
                                ilGen.Emit(OpCodes.Call, method);
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
