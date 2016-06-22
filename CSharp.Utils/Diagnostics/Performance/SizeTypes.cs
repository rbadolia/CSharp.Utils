using System;

namespace CSharp.Utils.Diagnostics.Performance
{

    #region Enumerations

    [Flags]
    public enum SizeTypes
    {
        None = 1, 

        Bytes = 2, 

        KiloBytes = 4, 

        MegaBytes = 8, 

        GigaBytes = 16
    }

    #endregion Enumerations
}
