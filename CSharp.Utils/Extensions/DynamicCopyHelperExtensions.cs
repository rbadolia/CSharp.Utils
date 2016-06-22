using CSharp.Utils.Reflection;

namespace CSharp.Utils.Extensions
{
    public static class DynamicCopyHelperExtensions
    {
        #region Public Methods and Operators

        public static T DynamicCopy<T>(T source) where T : new()
        {
            var target = new T();
            DynamicCopyHelper<T, T>.Copy(source, target);
            return target;
        }

        public static void DynamicCopy<T>(T source, T target)
        {
            DynamicCopyHelper<T, T>.Copy(source, target);
        }

        public static void DynamicCopy<TSource, TTarget>(TSource source, TTarget target)
        {
            DynamicCopyHelper<TSource, TTarget>.Copy(source, target);
        }

        #endregion Public Methods and Operators
    }
}
