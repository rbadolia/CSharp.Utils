using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    internal static class MsilWriter
    {
        #region Static Fields

        private static readonly Dictionary<Type, MethodInfo> _emitMethods;

        #endregion Static Fields

        #region Constructors and Finalizers

        static MsilWriter()
        {
            _emitMethods = new Dictionary<Type, MethodInfo>();
            MethodInfo[] allMethods = typeof(ILGenerator).GetMethods();
            foreach (MethodInfo method in allMethods)
            {
                if (method.Name == "Emit")
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2)
                    {
                        _emitMethods.Add(parameters[1].ParameterType, method);
                    }
                }
            }
        }

        #endregion Constructors and Finalizers

        #region Methods

        internal static DynamicMethod BuildDynamicMethod(MethodInformation info)
        {
            Type returnType = ReflectionHelper.GetType(info.ReturnType);
            Type[] parameterTypes = getParameterTypes(info.ParameterTypes);
            var dynamicMethod = new DynamicMethod("DynamicMethod" + GeneralHelper.Identity, ReflectionHelper.GetType(info.ReturnType), parameterTypes, typeof(MsilWriter));

            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            buildMethodBody(ilGen, info);
            return dynamicMethod;
        }

        private static void buildMethodBody(ILGenerator ilGen, MethodInformation info)
        {
            foreach (string local in info.LocalVariableTypes)
            {
                ilGen.DeclareLocal(ReflectionHelper.GetType(local));
            }

            foreach (MsilInstruction instruction in info.Instructions)
            {
                if (instruction.Data == null)
                {
                    if (instruction.TypeName == null)
                    {
                        ilGen.Emit(instruction.OpCode);
                    }
                    else
                    {
                        Type classType = ReflectionHelper.GetType(instruction.TypeName);
                        if (instruction.MethodCallInfo == null)
                        {
                            _emitMethods[typeof(Type)].Invoke(ilGen, new object[] { instruction.OpCode, classType });
                        }
                        else
                        {
                            Type[] pTypes = getParameterTypes(instruction.MethodCallInfo.ParameterTypes);
                            if (instruction.MethodCallInfo.MethodName == ".ctor")
                            {
                                ConstructorInfo ci = classType.GetConstructor(pTypes);
                                _emitMethods[typeof(ConstructorInfo)].Invoke(ilGen, new object[] { instruction.OpCode, ci });
                            }
                            else
                            {
                                MethodInfo mi = classType.GetMethod(instruction.MethodCallInfo.MethodName, pTypes);
                                _emitMethods[typeof(MethodInfo)].Invoke(ilGen, new object[] { instruction.OpCode, mi });
                            }
                        }
                    }
                }
                else
                {
                    MethodInfo mi = null;
                    Type type = instruction.Data.GetType();
                    if (!_emitMethods.TryGetValue(type, out mi))
                    {
                        foreach (var kvp in _emitMethods)
                        {
                            if (kvp.Key.IsAssignableFrom(type))
                            {
                                mi = kvp.Value;
                                break;
                            }
                        }
                    }

                    if (mi == null)
                    {
                    }

                    mi.Invoke(ilGen, new[] { instruction.OpCode, instruction.Data });
                }
            }
        }

        private static Type[] getParameterTypes(IList<string> parameterTypes)
        {
            var result = new Type[parameterTypes.Count];
            for (int i = 0; i < parameterTypes.Count; i++)
            {
                result[i] = ReflectionHelper.GetType(parameterTypes[i]);
            }

            return result;
        }

        #endregion Methods
    }
}
