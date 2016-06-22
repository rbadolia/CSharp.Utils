namespace CSharp.Utils.Reflection.Internal
{
    internal class ValueTypeDefaultCheckHelper<T>
    {
        #region Methods

        internal static bool IsDefault(T value1)
        {
            return Equals(value1, default(T));
        }

        #endregion Methods
    }
}
