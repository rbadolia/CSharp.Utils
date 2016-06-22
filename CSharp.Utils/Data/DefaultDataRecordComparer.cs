using System;
using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data
{
    public sealed class DefaultDataRecordComparer : IComparer<IDataRecord>
    {
        #region Fields

        private readonly List<string> _keyColumnNames;

        #endregion Fields

        #region Constructors and Finalizers

        public DefaultDataRecordComparer(IEnumerable<string> keyColumnNames)
        {
            this._keyColumnNames = new List<string>(keyColumnNames);
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public int Compare(IDataRecord record1, IDataRecord record2)
        {
            foreach (string keyColumnName in this._keyColumnNames)
            {
                var c1 = record1[keyColumnName] as IComparable;
                var c2 = record2[keyColumnName] as IComparable;
                if (c1 != null && c2 != null)
                {
                    int cResult = c1.CompareTo(c2);
                    if (cResult != 0)
                    {
                        return cResult;
                    }
                }
                else
                {
                    if (c2 == null && c1 == null)
                    {
                        continue;
                    }

                    return c1 == null ? 1 : -1;
                }
            }

            return 0;
        }

        #endregion Public Methods and Operators
    }
}
