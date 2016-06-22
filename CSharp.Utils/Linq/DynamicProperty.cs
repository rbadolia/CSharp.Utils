using System;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Linq
{
    public class DynamicProperty
    {
        #region Fields

        private readonly string name;

        private readonly Type type;

        #endregion Fields

        #region Constructors

        public DynamicProperty(string name, Type type)
        {
            Guard.ArgumentNotNull(name, "name");
            Guard.ArgumentNotNull(type, "type");
            this.name = name;
            this.type = type;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public Type Type
        {
            get { return type; }
        }

        #endregion Properties
    }
}
