namespace CSharp.Utils.Validation
{
    public interface IValidator<in T>
    {
        #region Public Methods and Operators

        ValidationResult Validate(T obj);

        #endregion Public Methods and Operators
    }
}
