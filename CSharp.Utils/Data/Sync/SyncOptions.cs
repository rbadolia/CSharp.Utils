using System;

namespace CSharp.Utils.Data.Sync
{

    #region Enumerations

    [Flags]
    public enum SyncOptions
    {
        MissedRecords = 1, 

        MatchedRecords = 2, 
    }

    #endregion Enumerations
}
