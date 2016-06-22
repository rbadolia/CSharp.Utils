using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data.Helpers
{
    public class DbTypeMappingHelper
    {
        #region Static Fields

        private static readonly Dictionary<Type, DbType> typeMappings;

        #endregion Static Fields

        #region Constructors and Finalizers

        static DbTypeMappingHelper()
        {
            typeMappings = new Dictionary<Type, DbType>();
            typeMappings.Add(typeof(byte), DbType.Byte);
            typeMappings.Add(typeof(sbyte), DbType.SByte);
            typeMappings.Add(typeof(short), DbType.Int16);
            typeMappings.Add(typeof(ushort), DbType.UInt16);
            typeMappings.Add(typeof(int), DbType.Int32);
            typeMappings.Add(typeof(uint), DbType.UInt32);
            typeMappings.Add(typeof(long), DbType.Int64);
            typeMappings.Add(typeof(ulong), DbType.UInt64);
            typeMappings.Add(typeof(float), DbType.Single);
            typeMappings.Add(typeof(double), DbType.Double);
            typeMappings.Add(typeof(decimal), DbType.Decimal);
            typeMappings.Add(typeof(bool), DbType.Boolean);
            typeMappings.Add(typeof(string), DbType.String);
            typeMappings.Add(typeof(char), DbType.StringFixedLength);
            typeMappings.Add(typeof(Guid), DbType.Guid);
            typeMappings.Add(typeof(DateTime), DbType.DateTime);
            typeMappings.Add(typeof(DateTimeOffset), DbType.DateTimeOffset);
            typeMappings.Add(typeof(byte[]), DbType.Binary);
            typeMappings.Add(typeof(byte?), DbType.Byte);
            typeMappings.Add(typeof(sbyte?), DbType.SByte);
            typeMappings.Add(typeof(short?), DbType.Int16);
            typeMappings.Add(typeof(ushort?), DbType.UInt16);
            typeMappings.Add(typeof(int?), DbType.Int32);
            typeMappings.Add(typeof(uint?), DbType.UInt32);
            typeMappings.Add(typeof(long?), DbType.Int64);
            typeMappings.Add(typeof(ulong?), DbType.UInt64);
            typeMappings.Add(typeof(float?), DbType.Single);
            typeMappings.Add(typeof(double?), DbType.Double);
            typeMappings.Add(typeof(decimal?), DbType.Decimal);
            typeMappings.Add(typeof(bool?), DbType.Boolean);
            typeMappings.Add(typeof(char?), DbType.StringFixedLength);
            typeMappings.Add(typeof(Guid?), DbType.Guid);
            typeMappings.Add(typeof(DateTime?), DbType.DateTime);
            typeMappings.Add(typeof(DateTimeOffset?), DbType.DateTimeOffset);
            typeMappings.Add(typeof(Binary), DbType.Binary);
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public static bool TryGetDbType<T>(out DbType dbType)
        {
            return TryGetDbType(typeof(T), out dbType);
        }

        public static bool TryGetDbType(Type type, out DbType dbType)
        {
            Guard.ArgumentNotNull(type, "type");
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            return typeMappings.TryGetValue(type, out dbType);
        }

        #endregion Public Methods and Operators
    }
}
