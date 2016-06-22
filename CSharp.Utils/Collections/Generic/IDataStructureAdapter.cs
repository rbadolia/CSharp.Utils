namespace CSharp.Utils.Collections.Generic
{
    public interface IDataStructureAdapter<T>
    {
        #region Public Properties

        int Capacity { get; }

        #endregion Public Properties

        #region Public Indexers

        T this[int index] { get; set; }

        #endregion Public Indexers

        #region Public Methods and Operators

        void Clear();

        #endregion Public Methods and Operators
    }
}
