using System;
using System.Data;

namespace CSharp.Utils.Data
{
    public abstract class DataReaderBase : AbstractDisposable
    {
        #region Fields

        private readonly IDataReader adaptedObject;

        #endregion Fields

        #region Constructors and Finalizers

        protected DataReaderBase(IDataReader adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public virtual int Depth
        {
            get
            {
                return this.adaptedObject.Depth;
            }
        }

        public virtual int FieldCount
        {
            get
            {
                return this.adaptedObject.FieldCount;
            }
        }

        public virtual bool IsClosed
        {
            get
            {
                return this.adaptedObject.IsClosed;
            }
        }

        public long NumberOfRecordsReadSoFar { get; private set; }

        public virtual int RecordsAffected
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

        #region Public Methods and Operators

        public virtual void Close()
        {
            this.adaptedObject.Close();
        }

        public virtual byte GetByte(int i)
        {
            return this.adaptedObject.GetByte(i);
        }

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return this.adaptedObject.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
        }

        public virtual long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            return this.adaptedObject.GetChars(i, fieldOffset, buffer, bufferOffset, length);
        }

        public virtual IDataReader GetData(int i)
        {
            return this.adaptedObject.GetData(i);
        }

        public virtual string GetDataTypeName(int i)
        {
            return this.adaptedObject.GetDataTypeName(i);
        }

        public virtual Type GetFieldType(int i)
        {
            return this.adaptedObject.GetFieldType(i);
        }

        public virtual string GetName(int i)
        {
            return this.adaptedObject.GetName(i);
        }

        public virtual int GetOrdinal(string name)
        {
            return this.adaptedObject.GetOrdinal(name);
        }

        public virtual DataTable GetSchemaTable()
        {
            return this.adaptedObject.GetSchemaTable();
        }

        public virtual bool IsDBNull(int i)
        {
            return this.adaptedObject.IsDBNull(i);
        }

        public virtual bool NextResult()
        {
            return this.adaptedObject.NextResult();
        }

        public bool Read()
        {
            bool couldRead = this.readProtected();
            if (couldRead)
            {
                this.NumberOfRecordsReadSoFar++;
            }

            return couldRead;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.adaptedObject.Dispose();
        }

        protected virtual bool readProtected()
        {
            return this.adaptedObject.Read();
        }

        #endregion Methods
    }
}
