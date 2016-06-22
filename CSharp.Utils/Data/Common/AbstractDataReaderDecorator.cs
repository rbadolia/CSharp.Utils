using System;
using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDataReaderDecorator : AbstractDisposable, IDataReader
    {
        #region Fields

        private readonly IDataReader adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractDataReaderDecorator(IDataReader adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int Depth
        {
            get
            {
                return this.adaptedObject.Depth;
            }
        }

        public int FieldCount
        {
            get
            {
                return this.adaptedObject.FieldCount;
            }
        }

        public bool IsClosed
        {
            get
            {
                return this.adaptedObject.IsClosed;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return this.adaptedObject.RecordsAffected;
            }
        }

        #endregion Public Properties

        #region Properties

        protected IDataReader AdaptedObject
        {
            get
            {
                return this.adaptedObject;
            }
        }

        #endregion Properties

        #region Public Indexers

        public object this[string name]
        {
            get
            {
                return this.adaptedObject[name];
            }
        }

        public object this[int i]
        {
            get
            {
                return this.adaptedObject[i];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public void Close()
        {
            this.adaptedObject.Close();
        }

        public bool GetBoolean(int i)
        {
            return this.adaptedObject.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return this.adaptedObject.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return this.adaptedObject.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
        }

        public char GetChar(int i)
        {
            return this.adaptedObject.GetChar(i);
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            return this.adaptedObject.GetChars(i, fieldOffset, buffer, bufferOffset, length);
        }

        public IDataReader GetData(int i)
        {
            return this.adaptedObject.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return this.adaptedObject.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return this.adaptedObject.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return this.adaptedObject.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return this.adaptedObject.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return this.adaptedObject.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return this.adaptedObject.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return this.adaptedObject.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return this.adaptedObject.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return this.adaptedObject.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return this.adaptedObject.GetInt64(i);
        }

        public string GetName(int i)
        {
            return this.adaptedObject.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return this.adaptedObject.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return this.adaptedObject.GetSchemaTable();
        }

        public string GetString(int i)
        {
            return this.adaptedObject.GetString(i);
        }

        public object GetValue(int i)
        {
            return this.adaptedObject.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return this.adaptedObject.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return this.adaptedObject.IsDBNull(i);
        }

        public bool NextResult()
        {
            return this.adaptedObject.NextResult();
        }

        public abstract bool Read();

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.adaptedObject.Dispose();
        }

        #endregion Methods
    }
}
