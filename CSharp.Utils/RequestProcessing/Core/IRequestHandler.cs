namespace CSharp.Utils.RequestProcessing.Core
{
    public interface IRequestHandler
    {
        #region Public Methods and Operators

        IResponse ProcessRequest(IRequest request);

        #endregion Public Methods and Operators
    }
}
