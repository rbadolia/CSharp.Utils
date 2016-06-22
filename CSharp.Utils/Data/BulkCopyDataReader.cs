using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CSharp.Utils.Data
{
    public sealed class BulkCopyDataReader : IDataReader
    {
        #region Fields

        private readonly IEnumerator<DbParameterCollection> enumerator;

        private readonly Dictionary<string, int> indexes = new Dictionary<string, int>();

        #endregion Fields

        #region Constructors and Finalizers

        public BulkCopyDataReader(IList<DbParameterCollection> collection)
        {
            if (collection.Count > 0)
            {
                DbParameterCollection coll = collection[0];
                for (int i = 0; i < coll.Count; i++)
                {
                    this.indexes.Add(coll[i].ParameterName.ToUpper(), i);
                }

                this.FieldCount = coll.Count;
            }

            this.enumerator = collection.GetEnumerator();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Depth { get; private set; }

        public int FieldCount { get; private set; }

        public bool IsClosed
        {
            get
            {
                return false;
            }
        }

        public int RecordsAffected { get; private set; }

        #endregion Public Properties

        #region Public Indexers

        public object this[string name]
        {
            get
            {
                int index = this.indexes[name.ToUpper()];
                DbParameter parameter = this.enumerator.Current[index];
                return parameter.Value;
            }
        }

        public object this[int i]
        {
            get
            {
                return this.enumerator.Current[i].Value;
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            return (bool)this.enumerator.Current[i].Value;
        }

        public byte GetByte(int i)
        {
            return (byte)this.enumerator.Current[i].Value;
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return 0;
        }

        public char GetChar(int i)
        {
            return (char)this.enumerator.Current[i].Value;
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            return 0;
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            if (this.enumerator.Current[i].Value != null)
            {
                return this.enumerator.Current[i].Value.GetType().FullName;
            }

            return null;
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)this.enumerator.Current[i].Value;
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)this.enumerator.Current[i].Value;
        }

        public double GetDouble(int i)
        {
            return (double)this.enumerator.Current[i].Value;
        }

        public Type GetFieldType(int i)
        {
            if (this.enumerator.Current[i].Value != null)
            {
                return this.enumerator.Current[i].Value.GetType();
            }

            return null;
        }

        public float GetFloat(int i)
        {
            return (float)this.enumerator.Current[i].Value;
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this.enumerator.Current[i].Value;
        }

        public short GetInt16(int i)
        {
            return (short)this.enumerator.Current[i].Value;
        }

        public int GetInt32(int i)
        {
            return (int)this.enumerator.Current[i].Value;
        }

        public long GetInt64(int i)
        {
            return (long)this.enumerator.Current[i].Value;
        }

        public string GetName(int i)
        {
            return this.enumerator.Current[i].ParameterName;
        }

        public int GetOrdinal(string name)
        {
            return this.indexes[name.ToUpper()];
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public string GetString(int i)
        {
            return (string)this.enumerator.Current[i].Value;
        }

        public object GetValue(int i)
        {
            return this.enumerator.Current[i].Value;
        }

        public int GetValues(object[] values)
        {
            return 0;
        }

        public bool IsDBNull(int i)
        {
            return this.enumerator.Current[i].Value == null || this.enumerator.Current[i].Value == DBNull.Value;
        }

        public bool NextResult()
        {
            return true;
        }

        public bool Read()
        {
            return this.enumerator.MoveNext();
        }

        #endregion Public Methods and Operators
    }
}
