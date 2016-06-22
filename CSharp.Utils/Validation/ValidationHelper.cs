using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Validation
{
    public static class ValidationHelper
    {
        public static bool ValidateProperty(object propertyValue, object obj, string propertyName)
        {
            Validator.ValidateProperty(propertyValue, new ValidationContext(obj, null, null) {MemberName = propertyName});
            return true;
        }

        [CautionUsedByReflection]
        public static void ValidateObject(object obj)
        {
            Validator.ValidateObject(obj, new ValidationContext(obj, null, null), true);
        }

        public static void InjectInputValidationOnType<T>()
        {
            InjectInputValidationOnType(typeof (T));
        }

        public static void InjectInputValidationOnType(Type type)
        {
            var stringType = typeof (string);
            var voidType = typeof (void);
            var validateObjectMethodInfo = typeof (ValidationHelper).GetMethod("ValidateObject", 
                BindingFlags.Public | BindingFlags.Static);

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.IsAbstract)
                {
                    continue;
                }

                var attributes = ReflectionHelper.GetAttributes<CommandAttribute>(method, true, true);
                if (attributes.Count == 0)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                Type[] parameterTypes;
                int destIndex = 0;
                if (!method.IsStatic)
                {
                    parameterTypes = new Type[parameters.Length + 1];
                    parameterTypes[0] = method.DeclaringType;
                    destIndex = 1;
                }
                else
                {
                    parameterTypes = new Type[parameters.Length];
                }

                var parameterIndexesToValidate = new List<int>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    parameterTypes[i + destIndex] = parameterType;

                    if (parameterType.IsPrimitive || parameterType.IsValueType || parameterType == stringType)
                    {
                        continue;
                    }

                    parameterIndexesToValidate.Add(i + destIndex);
                }

                if (parameterIndexesToValidate.Count == 0)
                {
                    continue;
                }

                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var injectedMethodName = method.Name + "_" + GeneralHelper.Identity.ToString();

                var dynamicMethod = new DynamicMethod(injectedMethodName, method.ReturnType, parameterTypes, 
                    type);
                ILGenerator ilGen = dynamicMethod.GetILGenerator();

                if (method.ReturnType != voidType)
                {
                    ilGen.DeclareLocal(method.ReturnType);
                }

                foreach (var index in parameterIndexesToValidate)
                {
                    ilGen.Emit(OpCodes.Ldarg, index);
                    ilGen.Emit(OpCodes.Call, validateObjectMethodInfo);
                }

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldarg, i);
                }

                ilGen.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);

                if (method.ReturnType != voidType)
                {
                    ilGen.Emit(OpCodes.Stloc, 0);
                }

                ilGen.Emit(OpCodes.Ret);

                MethodUtil.ReplaceMethod(dynamicMethod, method);
            }
        }
    }
}
