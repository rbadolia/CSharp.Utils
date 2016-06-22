using System;
using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDataRecord : IDataRecord
    {
        #region Fields

        private Dictionary<string, int> ordinalsDictionary;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractDataRecord()
        {
            this.ColumnNames = new List<string>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int FieldCount
        {
            get
            {
                return this.ColumnNames.Count;
            }
        }

        #endregion Public Properties

        #region Properties

        protected List<string> ColumnNames { get; private set; }

        #endregion Properties

        #region Public Indexers

        public abstract object this[int i] { get; }

        public object this[string name]
        {
            get
            {
                int index = this.GetOrdinal(name);
                return this[index];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public virtual bool GetBoolean(int i)
        {
            return this.getValue<bool>(i);
        }

        public virtual byte GetByte(int i)
        {
            return this.getValue<byte>(i);
        }

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return 0;
        }

        public virtual char GetChar(int i)
        {
            return this.getValue<char>(i);
        }

        public virtual long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            return 0;
        }

        public virtual IDataReader GetData(int i)
        {
            return null;
        }

        public virtual string GetDataTypeName(int i)
        {
            Type t = this.GetFieldType(i);
            if (t == null)
            {
                return null;
            }

            return t.ToString();
        }

        public virtual DateTime GetDateTime(int i)
        {
            return this.getValue<DateTime>(i);
        }

        public virtual decimal GetDecimal(int i)
        {
            return this.getValue<decimal>(i);
        }

        public virtual double GetDouble(int i)
        {
            return this.getValue<double>(i);
        }

        public virtual Type GetFieldType(int i)
        {
            return typeof(object);
        }

        public virtual float GetFloat(int i)
        {
            return this.getValue<float>(i);
        }

        public virtual Guid GetGuid(int i)
        {
            return this.getValue<Guid>(i);
        }

        public virtual short GetInt16(int i)
        {
            return this.getValue<short>(i);
        }

        public virtual int GetInt32(int i)
        {
            return this.getValue<int>(i);
        }

        public virtual long GetInt64(int i)
        {
            return this.getValue<long>(i);
        }

        public string GetName(int i)
        {
            return this.ColumnNames[i];
        }

        public int GetOrdinal(string name)
        {
            if (this.ordinalsDictionary == null)
            {
                this.ordinalsDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < this.ColumnNames.Count; i++)
                {
                    this.ordinalsDictionary.Add(this.ColumnNames[i], i);
                }
            }

            return this.ordinalsDictionary[name];
        }

        public virtual string GetString(int i)
        {
            return (string)this[i];
        }

        public object GetValue(int i)
        {
            return this[i];
        }

        public virtual int GetValues(object[] values)
        {
            for (int i = 0; i < this.FieldCount; i++)
            {
                values[i] = this[i];
            }

            return 0;
        }

        public virtual bool IsDBNull(int i)
        {
            if (this.GetFieldType(i).IsValueType)
            {
                return false;
            }

            return this[i] == null || this[i] == DBNull.Value;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual T getValue<T>(int i)
        {
            return (T)this[i];
        }

        #endregion Methods
    }
}
