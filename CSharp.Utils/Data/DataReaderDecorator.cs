using System;
using System.Data;

namespace CSharp.Utils.Data
{
    public class DataReaderDecorator : DataReaderBase, IDataReader
    {
        #region Constructors and Finalizers

        public DataReaderDecorator(IDataReader adaptedObject)
            : base(adaptedObject)
        {
        }

        #endregion Constructors and Finalizers

        #region Public Indexers

        public virtual object this[string name]
        {
            get
            {
                return this.AdaptedObject[name];
            }
        }

        public virtual object this[int i]
        {
            get
            {
                return this.AdaptedObject[i];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public virtual bool GetBoolean(int i)
        {
            return this.AdaptedObject.GetBoolean(i);
        }

        public virtual char GetChar(int i)
        {
            return this.AdaptedObject.GetChar(i);
        }

        public virtual DateTime GetDateTime(int i)
        {
            return this.AdaptedObject.GetDateTime(i);
        }

        public virtual decimal GetDecimal(int i)
        {
            return this.AdaptedObject.GetDecimal(i);
        }

        public virtual double GetDouble(int i)
        {
            return this.AdaptedObject.GetDouble(i);
        }

        public virtual float GetFloat(int i)
        {
            return this.AdaptedObject.GetFloat(i);
        }

        public virtual Guid GetGuid(int i)
        {
            return this.AdaptedObject.GetGuid(i);
        }

        public virtual short GetInt16(int i)
        {
            return this.AdaptedObject.GetInt16(i);
        }

        public virtual int GetInt32(int i)
        {
            return this.AdaptedObject.GetInt32(i);
        }

        public virtual long GetInt64(int i)
        {
            return this.AdaptedObject.GetInt64(i);
        }

        public virtual string GetString(int i)
        {
            return this.AdaptedObject.GetString(i);
        }

        public virtual object GetValue(int i)
        {
            return this.AdaptedObject.GetValue(i);
        }

        public virtual int GetValues(object[] values)
        {
            return this.AdaptedObject.GetValues(values);
        }

        #endregion Public Methods and Operators
    }
}
