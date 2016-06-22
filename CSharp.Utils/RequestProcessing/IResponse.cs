using System;

namespace CSharp.Utils.RequestProcessing
{
    public interface IResponse
    {
        #region Public Properties

        string Message { get; set; }

        Exception Exception { get; set; }

        string RequestId { get; set; }

        ProcessingResult Result { get; set; }

        #endregion
    }
}
