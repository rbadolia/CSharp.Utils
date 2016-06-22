using System;
using System.Data;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractDataReader : AbstractDataRecord, IDataReader
    {
        #region Public Properties

        public virtual int Depth { get; protected set; }

        public virtual bool IsClosed
        {
            get
            {
                return false;
            }
        }

        public virtual int RecordsAffected { get; protected set; }

        public long RecordsReadSoFar { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public virtual void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override IDataReader GetData(int i)
        {
            return this;
        }

        public virtual DataTable GetSchemaTable()
        {
            return null;
        }

        public virtual bool NextResult()
        {
            return true;
        }

        public bool Read()
        {
            bool couldRead = this.readProtected();
            if (couldRead)
            {
                this.RecordsReadSoFar++;
            }

            return couldRead;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual void Dispose(bool disposing)
        {
        }

        protected abstract bool readProtected();

        #endregion Methods
    }
}
