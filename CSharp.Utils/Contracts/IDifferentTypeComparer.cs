namespace CSharp.Utils.Contracts
{
    public interface IDifferentTypeComparer<in T1, in T2>
    {
        #region Public Methods and Operators

        int Compare(T1 x, T2 y);

        #endregion Public Methods and Operators
    }
}
