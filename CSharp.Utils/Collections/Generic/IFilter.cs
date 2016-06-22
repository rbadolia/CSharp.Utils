namespace CSharp.Utils.Collections.Generic
{
    public interface IFilter<in T>
    {
        #region Public Methods and Operators

        bool ShouldFilter(T obj);

        #endregion Public Methods and Operators
    }
}
