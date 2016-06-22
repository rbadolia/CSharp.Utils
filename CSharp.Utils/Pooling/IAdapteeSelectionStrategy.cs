namespace CSharp.Utils.Pooling
{
    public interface IAdaptedObjectSelectionStrategy<in T>
    {
        #region Public Methods and Operators

        bool CanSelectThisAdaptedObject(T adaptedObject, bool isNewlyCreatedAdaptedObject, object tag);

        #endregion Public Methods and Operators
    }
}
