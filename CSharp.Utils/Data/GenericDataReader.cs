using System;
using CSharp.Utils.Data.Common;

namespace CSharp.Utils.Data
{
    public sealed class GenericDataReader : AbstractDataReader
    {
        #region Fields

        private IDataReaderStrategy _strategy;

        #endregion Fields

        #region Constructors and Finalizers

        public GenericDataReader(IDataReaderStrategy strategy)
        {
            this.Strategy = strategy;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public IDataReaderStrategy Strategy
        {
            get
            {
                return this._strategy;
            }

            set
            {
                this._strategy = value;
                this.ColumnNames.Clear();
                this.ColumnNames.AddRange(this._strategy.ColumnNames);
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public override object this[int i]
        {
            get
            {
                return this._strategy[i];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public override Type GetFieldType(int i)
        {
            Type fieldType = this._strategy.GetFieldType(i);
            return fieldType ?? base.GetFieldType(i);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._strategy.Dispose();
        }

        protected override bool readProtected()
        {
            return this._strategy.Read();
        }

        #endregion Methods
    }
}
