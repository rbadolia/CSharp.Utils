namespace CSharp.Utils.Reflection.CodeSerialization
{
    public sealed class IgnoreAssemblyVersionEqualityComparer : AbstractEqualityComparer<string>
    {
        #region Static Fields

        private static readonly IgnoreAssemblyVersionEqualityComparer InstanceObject = new IgnoreAssemblyVersionEqualityComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private IgnoreAssemblyVersionEqualityComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static IgnoreAssemblyVersionEqualityComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        protected override bool EqualsProtected(string x, string y)
        {
            if (x == y)
            {
                return true;
            }

            var parserX = new TypeParser(x);
            var parserY = new TypeParser(y);
            if (parserX.ShortAssemblyName == parserY.ShortAssemblyName)
            {
                if (parserX.ShortTypeName == parserY.ShortTypeName)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode(string obj)
        {
            var parser = new TypeParser(obj);
            return (parser.ShortAssemblyName + "\\" + parser.ShortTypeName).GetHashCode();
        }

        #endregion Public Methods and Operators
    }
}
