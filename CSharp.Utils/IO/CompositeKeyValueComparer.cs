using System;
using System.Collections.Generic;
using System.IO;
using CSharp.Utils.Collections.Concurrent;

namespace CSharp.Utils.IO
{
    public sealed class CompositeKeyValueComparer : IComparer<CompositeKeyValue<string, FileInfo>>
    {
        #region Static Fields

        private static readonly CompositeKeyValueComparer InstanceObject = new CompositeKeyValueComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private CompositeKeyValueComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static CompositeKeyValueComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(CompositeKeyValue<string, FileInfo> x, CompositeKeyValue<string, FileInfo> y)
        {
            int cmp;
            if (x.Value != null && y.Value != null)
            {
                cmp = DateTime.Compare(x.Value.LastWriteTime, y.Value.LastWriteTime);
                return -cmp;
            }

            cmp = string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
            return cmp != 0 ? cmp : (x.Value == null ? 1 : -1);
        }

        #endregion Public Methods and Operators
    }
}
