namespace CSharp.Utils
{
    public sealed class ByteArrayEqualityComparer : AbstractEqualityComparer<byte[]>
    {
        private static ByteArrayEqualityComparer _instance = new ByteArrayEqualityComparer();

        private ByteArrayEqualityComparer()
        {

        }

        public static ByteArrayEqualityComparer Instance
        {
            get { return _instance; }
        }

        protected override bool EqualsProtected(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            int length = x.Length;
            for (int i = 0; i < length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
