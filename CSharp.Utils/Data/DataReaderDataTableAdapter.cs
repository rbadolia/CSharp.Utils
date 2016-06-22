using System;
using System.Data;

namespace CSharp.Utils.Data
{
    public class DataReaderDataTableAdapter
    {
        #region Fields

        private readonly string name;

        private readonly IDataReader reader;

        private readonly DataTable table;

        private int rowIndex = -1;

        #endregion Fields

        #region Constructors and Finalizers

        public DataReaderDataTableAdapter(IDataReader reader, string name)
        {
            this.reader = reader;
            this.name = name;
        }

        public DataReaderDataTableAdapter(DataTable table)
        {
            this.table = table;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int ColumnCount
        {
            get
            {
                return this.reader == null ? this.table.Columns.Count : this.reader.FieldCount;
            }
        }

        public string Name
        {
            get
            {
                return this.reader == null ? this.table.TableName : this.name;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public string GetColumnName(int i)
        {
            return this.reader == null ? this.table.Columns[i].ColumnName : this.reader.GetName(i);
        }

        public Type GetColumnType(int i)
        {
            return this.reader == null ? this.table.Columns[i].DataType : this.reader.GetFieldType(i);
        }

        public object GetValue(int i)
        {
            return this.reader == null ? this.table.Rows[this.rowIndex][i] : this.reader[i];
        }

        public bool Read()
        {
            if (this.reader != null)
            {
                bool canRead = this.reader.Read();
                if (!canRead)
                {
                    this.reader.Dispose();
                }

                return canRead;
            }

            this.rowIndex++;
            return this.rowIndex < this.table.Rows.Count;
        }

        #endregion Public Methods and Operators
    }
}
