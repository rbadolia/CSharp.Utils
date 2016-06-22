using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    [Serializable]
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [AspNetHostingPermission(SecurityAction.LinkDemand, 
        Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>, ISerializable
    {
        private delegate bool DynamicMethodDelegate(T x, T y);

        private static readonly DynamicMethodDelegate _methodDelegate;

        private static DynamicEqualityComparer<T> _instance = new DynamicEqualityComparer<T>();

        static DynamicEqualityComparer()
        {
            var parameterTypes = new[] {typeof (T), typeof (T)};
            Label? nextLabel = null;
            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties<T>(false);
            var dynamicMethod = new DynamicMethod("DynamicEquals", typeof (bool), parameterTypes, 
                typeof (DynamicEqualityComparer<T>));
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                if (property.IsDefined(typeof (ExcludeInEqualityComparisonAttribute)))
                {
                    continue;
                }

                if (nextLabel != null)
                {
                    ilGen.MarkLabel(nextLabel.Value);
                }

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                if (property.PropertyType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, property.PropertyType);
                }

                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Callvirt, property.GetMethod);
                if (property.PropertyType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, property.PropertyType);
                }

                ilGen.Emit(OpCodes.Call, SharedReflectionInfo.CompareMethod);

                ilGen.Emit(OpCodes.Ldc_I4_0);
                ilGen.Emit(OpCodes.Ceq);
                nextLabel = ilGen.DefineLabel();
                ilGen.Emit(OpCodes.Brtrue, nextLabel.Value);

                ilGen.Emit(OpCodes.Ldc_I4, 0);

                ilGen.Emit(OpCodes.Ret);
            }

            if (nextLabel != null)
            {
                ilGen.MarkLabel(nextLabel.Value);
            }

            ilGen.Emit(OpCodes.Ldc_I4, 1);
            ilGen.Emit(OpCodes.Ret);
            _methodDelegate = (DynamicMethodDelegate) dynamicMethod.CreateDelegate(typeof (DynamicMethodDelegate));
        }

        private DynamicEqualityComparer()
        {
        }

        public static DynamicEqualityComparer<T> Instance
        {
            get { return _instance; }
        }

        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if ((x == null && y != null) || (y == null && x != null))
            {
                return false;
            }

            var result = _methodDelegate(x, y);

            return result;
        }

        public int GetHashCode(T obj)
        {
            return DynamicToStringHelper<T>.ExportAsString(obj).GetHashCode();
        }

        [SecurityPermission(SecurityAction.LinkDemand, 
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof (DynamicEqualityComparerSerializationHelper<T>));
        }
    }
}
