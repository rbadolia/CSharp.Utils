using CSharp.Utils.RequestProcessing.Core;

namespace CSharp.Utils.RequestProcessing
{
    public interface IRequestHandler<in TRequest> : IRequestHandler
        where TRequest : IRequest
    {
        #region Public Methods and Operators

        IResponse ProcessRequest(TRequest request);

        #endregion Public Methods and Operators
    }
}
