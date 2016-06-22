using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharp.Utils.Data.Sync
{
    public abstract class AbstractDbTableSync : AbstractDisposable
    {
        #region Public Properties

        public Exception Exception { get; private set; }

        #endregion Public Properties

        #region Methods

        protected abstract IComparer<IDataRecord> getComparer();

        protected abstract IDataReader getDataFromSource(int sourceNumber);

        protected virtual void performPostSyncOfMatchedRecords(int sourceNumber)
        {
        }

        protected virtual void performPostSyncOfMismatchedRecords(int sourceNumber)
        {
        }

        protected void PerformSynchronization(SyncOptions options, SyncDirections directions)
        {
            IDataReader reader1 = this.getDataFromSource(1);
            IDataReader reader2 = this.getDataFromSource(2);
            IComparer<IDataRecord> comparer = this.getComparer();
            var sync = new DataReaderSync(reader1, reader2, comparer, options, directions);
            if (sync.ReaderForMissedRecordsInSource1 != null)
            {
                this.performSyncAsync(sync.ReaderForMissedRecordsInSource1, 1, true);
            }

            if (sync.ReaderForMissedRecordsInSource2 != null)
            {
                this.performSyncAsync(sync.ReaderForMissedRecordsInSource2, 2, true);
            }

            if (sync.ReaderForMatchedRecordsInSource1 != null)
            {
                this.performSyncAsync(sync.ReaderForMatchedRecordsInSource1, 2, false);
            }

            if (sync.ReaderForMatchedRecordsInSource2 != null)
            {
                this.performSyncAsync(sync.ReaderForMatchedRecordsInSource2, 1, false);
            }

            try
            {
                sync.StartSync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        protected virtual void startWriteOperationIntoSourceForMatchedRecords(IDataReader reader, int sourceNumber)
        {
            while (reader.Read())
            {
            }
        }

        protected virtual void startWriteOperationIntoSourceForMissedRecords(IDataReader reader, int sourceNumber)
        {
            while (reader.Read())
            {
            }
        }

        private void performSyncAsync(IDataReader reader, int sourceNumber, bool forMissedRecords)
        {
            Task.Factory.StartNew(delegate
                {
                    if (forMissedRecords)
                    {
                        this.startWriteOperationIntoSourceForMissedRecords(reader, sourceNumber);
                    }
                    else
                    {
                        this.startWriteOperationIntoSourceForMatchedRecords(reader, sourceNumber);
                    }

                    if (forMissedRecords)
                    {
                        this.performPostSyncOfMismatchedRecords(sourceNumber);
                    }
                    else
                    {
                        this.performPostSyncOfMatchedRecords(sourceNumber);
                    }
                });
        }

        #endregion Methods
    }
}
