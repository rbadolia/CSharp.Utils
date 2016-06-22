using System.Data;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Data.Common;

namespace CSharp.Utils.Data
{
    public class FilteredDataReader : AbstractDataReaderDecorator
    {
        #region Fields

        private readonly IFilter<IDataRecord> _recordFilter;

        #endregion Fields

        #region Constructors and Finalizers

        public FilteredDataReader(IDataReader adaptedObject, IFilter<IDataRecord> recordFilter)
            : base(adaptedObject)
        {
            this._recordFilter = recordFilter;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public override bool Read()
        {
            bool canRead = false;
            while (this.AdaptedObject.Read())
            {
                if (this._recordFilter.ShouldFilter(this.AdaptedObject))
                {
                    canRead = true;
                    break;
                }
            }

            return canRead;
        }

        #endregion Public Methods and Operators
    }
}
