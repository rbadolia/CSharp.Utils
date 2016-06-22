using System;

namespace CSharp.Utils.Diagnostics.Performance
{

    #region Enumerations

    [Flags]
    public enum TimeTypes
    {
        None = 1, 

        Ticks = 2, 

        Milliseconds = 4, 

        Seconds = 8, 

        Minutes = 16, 

        Hours = 32, 
    }

    #endregion Enumerations
}
