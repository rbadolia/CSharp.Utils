using System;
using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public interface IDataReaderStrategy : IDisposable
    {
        #region Public Properties

        ICollection<string> ColumnNames { get; }

        long TotalRecordCountIfKnown { get; }

        #endregion Public Properties

        #region Public Indexers

        object this[int i] { get; }

        #endregion Public Indexers

        #region Public Methods and Operators

        Type GetFieldType(int i);

        bool Read();

        #endregion Public Methods and Operators
    }
}
