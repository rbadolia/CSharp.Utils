using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using CSharp.Utils.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Validation
{
    public static class DynamicRequiredPropertyValidationHelper<T>
    {
        #region Static Fields

        private static readonly DynamicMethodDelegate _methodDelegate;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DynamicRequiredPropertyValidationHelper()
        {
            _methodDelegate = buildDynamicMethod();
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void DynamicMethodDelegate(T item, int index, List<ValidationResult> errors);

        #endregion Delegates

        #region Public Methods and Operators

        public static List<ValidationResult> Validate(T items, bool trimStrings = false)
        {
            List<ValidationResult> errors = Validate(new[] { items }, trimStrings);
            foreach (ValidationResult error in errors)
            {
                error.ItemSequenceNumber = null;
            }

            return errors;
        }

        public static List<ValidationResult> Validate(IList<T> items, bool trimStrings = false)
        {
            var errors = new List<ValidationResult>();
            for (int i = 0; i < items.Count; i++)
            {
                if (trimStrings)
                {
                    DynamicStringTrimHelper<T>.TrimStrings(items[i]);
                }

                _methodDelegate(items[i], i, errors);
            }

            return errors;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static DynamicMethodDelegate buildDynamicMethod()
        {
            Type objectType = typeof(T);
            var dynamicMethod = new DynamicMethod("DynamicValidate", typeof(void), new[] { objectType, typeof(int), typeof(List<ValidationResult>) }, typeof(DynamicRequiredPropertyValidationHelper<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, ColumnOrderComparer.Instance);
            Label returnLabel = ilGen.DefineLabel();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    var readonlyAttrib = property.GetCustomAttribute<ReadOnlyAttribute>(false);
                    if (readonlyAttrib != null && readonlyAttrib.IsReadOnly)
                    {
                        continue;
                    }

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                    Type t = typeof(ValueTypeDefaultCheckHelper<>).MakeGenericType(property.PropertyType);
                    MethodInfo mi = t.GetMethod("IsDefault", BindingFlags.Static | BindingFlags.NonPublic);
                    ilGen.Emit(OpCodes.Call, mi);
                    ilGen.Emit(OpCodes.Ldc_I4_1);
                    ilGen.Emit(OpCodes.Ceq);
                    Label nextLabel = ilGen.DefineLabel();
                    ilGen.Emit(OpCodes.Brtrue, nextLabel);

                    ilGen.Emit(OpCodes.Br, returnLabel);

                    ilGen.MarkLabel(nextLabel);
                    string propertyDisplayName = property.Name;
                    var displayNameAttrib = property.GetCustomAttribute<DisplayNameAttribute>(false);
                    if (displayNameAttrib != null && !string.IsNullOrEmpty(displayNameAttrib.DisplayName))
                    {
                        propertyDisplayName = displayNameAttrib.DisplayName;
                    }
                    else
                    {
                        var xmlElementAttrib = property.GetCustomAttribute<XmlElementAttribute>(false);
                        if (xmlElementAttrib != null && !string.IsNullOrEmpty(xmlElementAttrib.ElementName))
                        {
                            propertyDisplayName = xmlElementAttrib.ElementName;
                        }
                    }

                    ilGen.Emit(OpCodes.Ldstr, propertyDisplayName);
                    ilGen.Emit(OpCodes.Ldarg, 1);
                    ilGen.Emit(OpCodes.Ldarg, 2);
                    mi = typeof(MiscellaneousHelper).GetMethod("AddValidationError", BindingFlags.Static | BindingFlags.NonPublic);
                    ilGen.Emit(OpCodes.Call, mi);
                }
            }

            ilGen.Emit(OpCodes.Br, returnLabel);

            ilGen.MarkLabel(returnLabel);
            ilGen.Emit(OpCodes.Ret);
            var methodDelegate = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
            return methodDelegate;
        }

        #endregion Methods
    }
}
