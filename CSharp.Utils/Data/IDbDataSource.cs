namespace CSharp.Utils.Data
{
    public interface IDbDataSource<in T> : IDataSource<T>
    {
        #region Public Methods and Operators

        void SetDbTransactionContext(DbTransactionContext context);

        #endregion Public Methods and Operators
    }
}
