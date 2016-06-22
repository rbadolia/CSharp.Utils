using System;
using System.Collections.Generic;
using System.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Reflection
{
    public class AttributeBasedFilterTypesStrategy:IFilterTypesStrategy
    {
        public Type AttributeType
        {
            get; private set; 
        }

        public AttributeBasedFilterTypesStrategy(Type attributeType)
        {
            Guard.ArgumentNotNull(attributeType, "attributeType");
            if (!typeof(Attribute).IsAssignableFrom(attributeType))
            {
                throw new Exception(string.Format("The argument for attributeType {0} is not of a Attribute type.", attributeType));
            }

            this.AttributeType = attributeType;
        }

        public IEnumerable<Type> FilterTypes(Assembly assembly)
        {
            if (assembly.IsDefined(this.AttributeType))
            {
                foreach (var type in assembly.GetTypes())
                {
                    yield return type;
                }
            }
            else
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsDefined(this.AttributeType))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
