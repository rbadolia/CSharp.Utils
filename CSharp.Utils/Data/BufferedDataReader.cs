using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CSharp.Utils.Collections.Concurrent;

namespace CSharp.Utils.Data
{
    public class BufferedDataReader : DataReaderBase, IDataReader
    {
        #region Fields

        private readonly IDataReader adaptedObject;

        private readonly AutoResetEvent _consumerWaitHandle;

        private readonly AutoResetEvent _flushWaitHandle;

        private readonly ConcurrentStore<object[]> _store;

        private IEnumerator<object[]> _enumerator;

        private bool _hasCompleted;

        private volatile bool _isReadingAdaptedObjectCompleted;

        #endregion Fields

        #region Constructors and Finalizers

        public BufferedDataReader(IDataReader adaptedObject, int bufferSize, int periodicFlushInterval = -1)
            : base(adaptedObject)
        {
            this.adaptedObject = adaptedObject;
            this._consumerWaitHandle = new AutoResetEvent(false);
            this._flushWaitHandle = new AutoResetEvent(false);
            this._store = new ConcurrentStore<object[]>(bufferSize, periodicFlushInterval);
            this._store.OnStoreFlush += this._store_OnStoreFlush;
            ThreadPool.QueueUserWorkItem(this.readAdaptedObject);
        }

        #endregion Constructors and Finalizers

        #region Public Indexers

        public object this[string name]
        {
            get
            {
                int ordinal = this.adaptedObject.GetOrdinal(name);
                return this[ordinal];
            }
        }

        public object this[int i]
        {
            get
            {
                return this._enumerator.Current[i];
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public bool GetBoolean(int i)
        {
            return (bool)this[i];
        }

        public char GetChar(int i)
        {
            return (char)this[i];
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)this[i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)this[i];
        }

        public double GetDouble(int i)
        {
            return (double)this[i];
        }

        public float GetFloat(int i)
        {
            return (float)this[i];
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this[i];
        }

        public short GetInt16(int i)
        {
            return (short)this[i];
        }

        public int GetInt32(int i)
        {
            return (int)this[i];
        }

        public long GetInt64(int i)
        {
            return (long)this[i];
        }

        public string GetString(int i)
        {
            return (string)this[i];
        }

        public object GetValue(int i)
        {
            return this[i];
        }

        public int GetValues(object[] values)
        {
            this._enumerator.Current.CopyTo(values, 0);
            return this.FieldCount;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override bool readProtected()
        {
            while (true)
            {
                if (this._enumerator != null && this._enumerator.MoveNext())
                {
                    if (this._enumerator.Current == null)
                    {
                    }

                    return true;
                }

                if (this._hasCompleted)
                {
                    return false;
                }

                if (this._isReadingAdaptedObjectCompleted)
                {
                    this._hasCompleted = true;
                }

                this._flushWaitHandle.Set();
                this._consumerWaitHandle.WaitOne();
            }
        }

        private void _store_OnStoreFlush(object sender, StoreFlushEventArgs<object[]> e)
        {
            this._flushWaitHandle.WaitOne();
            this._enumerator = e.GetEnumerator();
            this._consumerWaitHandle.Set();
        }

        private void readAdaptedObject(object state)
        {
            while (this.adaptedObject.Read())
            {
                var array = new object[this.adaptedObject.FieldCount];
                this.adaptedObject.GetValues(array);
                this._store.Add(array);
            }

            this._store.ForceFlush();
            this._isReadingAdaptedObjectCompleted = true;
            this._consumerWaitHandle.Set();
        }

        #endregion Methods
    }
}
