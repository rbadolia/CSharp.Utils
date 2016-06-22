using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Reflection
{
    public static class MethodUtil
    {
        #region Static Fields

        private static readonly ConcurrentBag<Pair<MethodBase, MethodBase>> _references = new ConcurrentBag<Pair<MethodBase, MethodBase>>();

        #endregion Static Fields

        #region Public Methods and Operators

        public static IEnumerable<InjectableMethodInfo<T>> GetInjectableMethods<T>(Assembly assembly, Type dontInjectAttributeType = null) where T : Attribute
        {
            if (IsInjectableAssembly<T>(assembly))
            {
                if (dontInjectAttributeType == null || !Attribute.IsDefined(assembly, dontInjectAttributeType))
                {
                    var assemblyLevelAttribute = assembly.GetCustomAttribute(typeof(T)) as T;
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.FullName.Contains("+"))
                        {
                            continue;
                        }

                        if (dontInjectAttributeType == null || !Attribute.IsDefined(type, dontInjectAttributeType))
                        {
                            var typeLevelAttribute = type.GetCustomAttribute(typeof(T)) as T;
                            string methodName = null;
                            int increment = 0;
                            foreach (MethodInfo method in
                                type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                if (method.DeclaringType == type && !method.IsAbstract && !method.IsGenericMethod)
                                {
                                    if (dontInjectAttributeType == null || !Attribute.IsDefined(method, dontInjectAttributeType))
                                    {
                                        var methodLevelAttribute = method.GetCustomAttribute(typeof(T)) as T;
                                        T attributeToUse = methodLevelAttribute ?? (typeLevelAttribute ?? assemblyLevelAttribute);
                                        if (attributeToUse != null)
                                        {
                                            string name = method.Name;
                                            if (name == methodName)
                                            {
                                                increment++;
                                                methodName = name + increment.ToString(CultureInfo.InvariantCulture);
                                            }
                                            else
                                            {
                                                methodName = name;
                                            }

                                            var mInfo = new InjectableMethodInfo<T>(method, methodName, attributeToUse);
                                            yield return mInfo;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IntPtr GetMethodAddress(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                return GetDynamicMethodAddress(method);
            }

            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            return method.MethodHandle.GetFunctionPointer();
        }

        public static bool IsInjectableAssembly<T>(Assembly assembly) where T : Attribute
        {
            string attributeAssemblyName = typeof(T).Assembly.GetName().Name;
            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            int i = 0;
            for (; i < referencedAssemblies.Length; i++)
            {
                if (referencedAssemblies[i].Name == attributeAssemblyName)
                {
                    break;
                }
            }

            if (i >= referencedAssemblies.Length)
            {
                return false;
            }

            return true;
        }

        public static void ReplaceMethod(MethodBase newMethod, MethodBase oldMethod)
        {
            if (!MethodSignaturesEqual(newMethod, oldMethod))
            {
                throw new ArgumentException(@"The method signatures are not the same.", "newMethod");
            }

            _references.Add(new Pair<MethodBase, MethodBase>(newMethod, oldMethod));

            ReplaceMethod(GetMethodAddress(newMethod), oldMethod);
        }

        public static void ReplaceMethod(IntPtr srcAdr, MethodBase oldMethod)
        {
            IntPtr destAdr = GetMethodAddressRef(oldMethod);
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    var d = (ulong*)destAdr.ToPointer();
                    *d = (ulong)srcAdr.ToInt64();
                }
                else
                {
                    var d = (uint*)destAdr.ToPointer();
                    *d = (uint)srcAdr.ToInt32();
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private static IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                RuntimeMethodHandle handle = GetDynamicMethodRuntimeHandle(method);
                var ptr = (byte*)handle.Value.ToPointer();

                if (IsNet20Sp2OrGreater())
                {
                    RuntimeHelpers.PrepareMethod(handle);
                    return handle.GetFunctionPointer();
                }

                if (IntPtr.Size == 8)
                {
                    var address = (ulong*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }
                else
                {
                    var address = (uint*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }
            }
        }

        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            RuntimeMethodHandle handle;

            if (Environment.Version.Major == 4)
            {
                MethodInfo getMethodDescriptorInfo = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance);
                handle = (RuntimeMethodHandle)getMethodDescriptorInfo.Invoke(method, null);
            }
            else
            {
                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
                handle = (RuntimeMethodHandle)fieldInfo.GetValue(method);
            }

            return handle;
        }

        private static IntPtr GetMethodAddress20SP2(MethodBase method)
        {
            unsafe
            {
                return new IntPtr((int*)method.MethodHandle.Value.ToPointer() + 2);
            }
        }

        private static IntPtr GetMethodAddressRef(MethodBase srcMethod)
        {
            if (srcMethod is DynamicMethod)
            {
                return GetDynamicMethodAddress(srcMethod);
            }

            if (srcMethod.Name == "get_IsInitialized")
            {
            }

            RuntimeHelpers.PrepareMethod(srcMethod.MethodHandle);

            IntPtr funcPointer = srcMethod.MethodHandle.GetFunctionPointer();
            if (IsNet20Sp2OrGreater())
            {
                IntPtr addrRef = GetMethodAddress20SP2(srcMethod);
                if (IsAddressValueMatch(addrRef, funcPointer))
                {
                    return addrRef;
                }

                unsafe
                {
                    var methodDesc = (UInt64*)srcMethod.MethodHandle.Value.ToPointer();
                    var index = (int)((*methodDesc >> 32) & 0xFF);
                    if (IntPtr.Size == 8)
                    {
                        var classStart = (ulong*)srcMethod.DeclaringType.TypeHandle.Value.ToPointer();
                        classStart += 10;
                        classStart = (ulong*)*classStart;
                        ulong* address = classStart + index;
                        addrRef = new IntPtr(address);
                    }
                    else
                    {
                        var classStart = (uint*)srcMethod.DeclaringType.TypeHandle.Value.ToPointer();
                        classStart += 10;
                        classStart = (uint*)*classStart;
                        uint* address = classStart + index;
                        addrRef = new IntPtr(address);
                    }

                    if (IsAddressValueMatch(addrRef, funcPointer))
                    {
                        return addrRef;
                    }

                    string error = string.Format("Method Injection Error: The address {0} 's value {1} doesn't match expected value: {2}", addrRef, *(IntPtr*)addrRef, funcPointer);
                    throw new InvalidOperationException(error);
                }
            }

            unsafe
            {
                const int skip = 10;
                var location = (UInt64*)srcMethod.MethodHandle.Value.ToPointer();
                var index = (int)((*location >> 32) & 0xFF);
                if (IntPtr.Size == 8)
                {
                    var classStart = (ulong*)srcMethod.DeclaringType.TypeHandle.Value.ToPointer();
                    ulong* address = classStart + index + skip;
                    return new IntPtr(address);
                }
                else
                {
                    var classStart = (uint*)srcMethod.DeclaringType.TypeHandle.Value.ToPointer();
                    uint* address = classStart + index + skip;
                    return new IntPtr(address);
                }
            }
        }

        private static Type GetMethodReturnType(MethodBase method)
        {
            var methodInfo = method as MethodInfo;
            if (methodInfo == null)
            {
                throw new ArgumentException(@"Unsupported MethodBase : " + method.GetType().Name, "method");
            }

            return methodInfo.ReturnType;
        }

        private static bool IsAddressValueMatch(IntPtr address, IntPtr value)
        {
            unsafe
            {
                IntPtr realValue = *(IntPtr*)address;
                return realValue == value;
            }
        }

        private static bool IsNet20Sp2OrGreater()
        {
            if (Environment.Version.Major == 4)
            {
                return true;
            }

            return Environment.Version.Major == FrameworkVersions.Net20SP2.Major && Environment.Version.MinorRevision >= FrameworkVersions.Net20SP2.MinorRevision;
        }

        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY)
            {
                return false;
            }

            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
            {
                if ((x.IsStatic && !x.IsStatic && xParams.Length == yParams.Length + 1 && xParams[0].ParameterType == x.DeclaringType) || (x.IsStatic && !y.IsStatic && yParams.Length == xParams.Length + 1 && yParams[0].ParameterType == y.DeclaringType))
                {
                    ParameterInfo[] min = x.IsStatic ? yParams : xParams;
                    ParameterInfo[] max = x.IsStatic ? xParams : yParams;
                    return !min.Where((t, i) => t.ParameterType != max[i + 1].ParameterType).Any();
                }
            }

            return true;
        }

        #endregion Methods
    }
}
