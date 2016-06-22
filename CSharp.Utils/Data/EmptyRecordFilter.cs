using System;
using System.Data;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Data
{
    public sealed class EmptyRecordFilter : IFilter<IDataRecord>
    {
        #region Static Fields

        private static readonly EmptyRecordFilter InstanceObject = new EmptyRecordFilter();

        #endregion Static Fields

        #region Constructors and Finalizers

        private EmptyRecordFilter()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static EmptyRecordFilter Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool ShouldFilter(IDataRecord obj)
        {
            for (int i = 0; i < obj.FieldCount; i++)
            {
                object o = obj[i];
                if (o != null && o != DBNull.Value)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Public Methods and Operators
    }
}
