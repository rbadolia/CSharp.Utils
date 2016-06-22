using System;
using System.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data.Helpers
{
    public sealed class PropertyOrdinalInfo
    {
        public PropertyOrdinalInfo(PropertyInfo property, int ordinal, Type inputDataType)
        {
            Guard.ArgumentNotNull(property, "property");
            Guard.ArgumentNotNull(inputDataType, "inputDataType");
            this.Property = property;
            this.Ordinal = ordinal;
            this.InputDataType = inputDataType;
        }

        public PropertyInfo Property { get; private set; }

        public int Ordinal { get; private set; }

        public Type InputDataType { get; private set; }
    }
}
