namespace CSharp.Utils.Data
{
    public interface IDbDataSink<in T> : IDataSink<T>
    {
        #region Public Methods and Operators

        void SetDbTransactionContext(DbTransactionContext context);

        #endregion Public Methods and Operators
    }
}
