using System;

namespace CSharp.Utils.RequestProcessing
{

    #region Enumerations

    [Serializable]
    public enum ProcessingResult
    {
        Fail = 0, 

        Success = 1, 

        Unknown = 2, 

        SuccessWithErrorsOrWarnings = 3, 
    }

    #endregion Enumerations
}
