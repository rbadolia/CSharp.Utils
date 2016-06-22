using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSharp.Utils.Data.Dynamic.Internal
{
    internal sealed class DbColumnComparer : IComparer<KeyValuePair<PropertyInfo, IDbColumnMetadata>>
    {
        #region Static Fields

        private static readonly DbColumnComparer InstanceObject = new DbColumnComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private DbColumnComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static DbColumnComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(KeyValuePair<PropertyInfo, IDbColumnMetadata> x, KeyValuePair<PropertyInfo, IDbColumnMetadata> y)
        {
            return string.Compare(x.Value.ColumnName, y.Value.ColumnName, StringComparison.Ordinal);
        }

        #endregion Public Methods and Operators
    }
}
