using System.Data;
using CSharp.Utils.Data.Common;

namespace CSharp.Utils.Data
{
    public sealed class DataTableToDataReaderAdapter : AbstractDataReader
    {
        #region Fields

        private readonly DataTable _table;

        private int _rowIndex = -1;

        #endregion Fields

        #region Constructors and Finalizers

        public DataTableToDataReaderAdapter(DataTable table)
        {
            this._table = table;
            this.ColumnNames.Clear();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                this.ColumnNames.Add(table.Columns[i].ColumnName);
            }
        }

        #endregion Constructors and Finalizers

        #region Public Indexers

        public override object this[int i]
        {
            get
            {
                return this._table.Rows[this._rowIndex][i];
            }
        }

        #endregion Public Indexers

        #region Methods

        protected override bool readProtected()
        {
            this._rowIndex++;
            return this._rowIndex < this._table.Rows.Count;
        }

        #endregion Methods
    }
}
