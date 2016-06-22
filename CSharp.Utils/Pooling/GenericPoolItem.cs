namespace CSharp.Utils.Pooling
{
    public class GenericPoolItem<T> : PoolItem<T>
    {
        #region Public Properties

        public T AdaptedObject
        {
            get
            {
                return this.AdaptedObjectProtected;
            }
        }

        #endregion Public Properties
    }
}
