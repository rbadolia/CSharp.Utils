using System.Collections.Generic;

namespace CSharp.Utils
{
    public sealed class TimeComparer : IComparer<Time>
    {
        #region Static Fields

        private static readonly TimeComparer InstanceObject = new TimeComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private TimeComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static TimeComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(Time x, Time y)
        {
            return Time.Compare(x, y);
        }

        #endregion Public Methods and Operators
    }
}
