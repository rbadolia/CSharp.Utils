using System;
using System.Collections.Generic;
using System.IO;

namespace CSharp.Utils.IO
{
    public sealed class LastWrittenDateComparer : IComparer<FileInfo>, IComparer<string>
    {
        #region Static Fields

        private static readonly LastWrittenDateComparer InstanceObject = new LastWrittenDateComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private LastWrittenDateComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static LastWrittenDateComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public int Compare(FileInfo x, FileInfo y)
        {
            return DateTime.Compare(x.LastWriteTime, y.LastWriteTime);
        }

        public int Compare(string x, string y)
        {
            var xf = new FileInfo(x);
            var yf = new FileInfo(y);
            return Compare(xf, yf);
        }

        #endregion Public Methods and Operators
    }
}
