using System;

namespace CSharp.Utils.Data.Sync
{

    #region Enumerations

    [Flags]
    public enum SyncDirections
    {
        Source1ToSource2 = 1, 

        Source2ToSource1 = 2
    }

    #endregion Enumerations
}
