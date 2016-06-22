using System;

namespace CSharp.Utils.RequestProcessing
{
    public interface IMetricsEnabledResponse : IResponse
    {
        #region Public Properties

        TimeSpan TimeTaken { get; set; }

        #endregion Public Properties
    }
}
