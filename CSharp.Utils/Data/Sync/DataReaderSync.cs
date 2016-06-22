using System.Collections.Generic;
using System.Data;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data.Sync
{
    public sealed class DataReaderSync : AbstractDisposable
    {
        #region Fields

        private readonly IComparer<IDataRecord> _comparer;

        private readonly AutoResetEvent[] _inputReaderWaitHandles = { new AutoResetEvent(false), new AutoResetEvent(false) };

        private readonly IDataReader[] _inputReaders = new IDataReader[2];

        private readonly AutoResetEvent[] _matchWaitHandles = new AutoResetEvent[2];

        private readonly AutoResetEvent[] _mismatchWaitHandles = new AutoResetEvent[2];

        #endregion Fields

        #region Constructors and Finalizers

        public DataReaderSync(IDataReader fromSource1, IDataReader fromSource2, IComparer<IDataRecord> comparer, SyncOptions options, SyncDirections directions)
        {
            Guard.ArgumentNotNull(fromSource1, "fromSource1");
            Guard.ArgumentNotNull(fromSource2, "fromSource2");
            Guard.ArgumentNotNull(comparer, "comparer");

            this.ReaderForMissedRecordsInSource2 = null;
            this._inputReaders[0] = fromSource1;
            this._inputReaders[1] = fromSource2;
            this._comparer = comparer;
            this.Directions = directions;
            this.Options = options;

            if (this.Options != SyncOptions.MatchedRecords)
            {
                if (this.Directions.HasFlag(SyncDirections.Source2ToSource1))
                {
                    this.ReaderForMissedRecordsInSource2 = new CallbackBasedDataReaderDecorator(this._inputReaders[0], this.CanReadForMismatchCallback);
                    this._mismatchWaitHandles[0] = new AutoResetEvent(false);
                }

                if (this.Directions.HasFlag(SyncDirections.Source1ToSource2))
                {
                    this.ReaderForMissedRecordsInSource1 = new CallbackBasedDataReaderDecorator(this._inputReaders[1], this.CanReadForMismatchCallback);
                    this._mismatchWaitHandles[1] = new AutoResetEvent(false);
                }
            }

            if (this.Options != SyncOptions.MissedRecords)
            {
                if (this.Directions.HasFlag(SyncDirections.Source2ToSource1))
                {
                    this.ReaderForMatchedRecordsInSource2 = new CallbackBasedDataReaderDecorator(this._inputReaders[1], this.CanReadForMatchCallback);
                    this._matchWaitHandles[1] = new AutoResetEvent(false);
                }

                if (this.Directions.HasFlag(SyncDirections.Source1ToSource2))
                {
                    this.ReaderForMatchedRecordsInSource1 = new CallbackBasedDataReaderDecorator(this._inputReaders[0], this.CanReadForMatchCallback);
                    this._matchWaitHandles[0] = new AutoResetEvent(false);
                }
            }
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public SyncDirections Directions { get; private set; }

        public long NumberOfMatchingRecords { get; private set; }

        public long NumberOfRecordsMissedInSource1 { get; private set; }

        public long NumberOfRecordsMissedInSource2 { get; private set; }

        public long NumberOfRecordsReadFromSource1 { get; private set; }

        public long NumberOfRecordsReadFromSource2 { get; private set; }

        public SyncOptions Options { get; private set; }

        public IDataReader ReaderForMatchedRecordsInSource1 { get; private set; }

        public IDataReader ReaderForMatchedRecordsInSource2 { get; private set; }

        public IDataReader ReaderForMissedRecordsInSource1 { get; private set; }

        public IDataReader ReaderForMissedRecordsInSource2 { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void StartSync()
        {
            int compareResult = 0;
            while (true)
            {
                if (compareResult <= 0)
                {
                    this.readInputReader(0);
                }

                if (compareResult >= 0)
                {
                    this.readInputReader(1);
                }

                if (this._inputReaders[0].IsClosed && this._inputReaders[1].IsClosed)
                {
                    return;
                }

                compareResult = this.CompareDataRecords();
                if (compareResult == 0)
                {
                    this.NumberOfMatchingRecords++;
                    if (this.Options.HasFlag(SyncOptions.MissedRecords))
                    {
                        if (this.Directions.HasFlag(SyncDirections.Source1ToSource2))
                        {
                            WaitHandle.SignalAndWait(this._inputReaderWaitHandles[0], this._matchWaitHandles[0]);
                        }

                        if (this.Directions.HasFlag(SyncDirections.Source2ToSource1))
                        {
                            WaitHandle.SignalAndWait(this._inputReaderWaitHandles[1], this._matchWaitHandles[1]);
                        }
                    }

                    continue;
                }

                int index = compareResult < 0 ? 0 : 1;

                if (index == 0)
                {
                    this.NumberOfRecordsMissedInSource2++;
                }
                else
                {
                    this.NumberOfRecordsMissedInSource1++;
                }

                if (this.Options.HasFlag(SyncOptions.MissedRecords))
                {
                    if ((index == 0 && this.Directions.HasFlag(SyncDirections.Source1ToSource2)) || (index == 1 && this.Directions.HasFlag(SyncDirections.Source2ToSource1)))
                    {
                        WaitHandle.SignalAndWait(this._inputReaderWaitHandles[index], this._mismatchWaitHandles[index]);
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            GeneralHelper.DisposeIDisposables(this._inputReaderWaitHandles);
            GeneralHelper.DisposeIDisposables(this._mismatchWaitHandles);
            GeneralHelper.DisposeIDisposables(this._matchWaitHandles);
            GeneralHelper.DisposeIDisposables(this._matchWaitHandles);
            GeneralHelper.DisposeIDisposables(this._inputReaders);
        }

        private bool CanReadForMatchCallback(IDataReader reader)
        {
            int index = reader == this.ReaderForMatchedRecordsInSource1 ? 0 : 1;
            if (this._inputReaders[index].IsClosed)
            {
                return false;
            }

            WaitHandle.SignalAndWait(this._matchWaitHandles[index], this._inputReaderWaitHandles[index]);
            bool canread = !this._inputReaders[0].IsClosed && !this._inputReaders[1].IsClosed;
            return canread;
        }

        private bool CanReadForMismatchCallback(IDataReader reader)
        {
            int index = reader == this.ReaderForMissedRecordsInSource2 ? 0 : 1;
            if (this._inputReaders[index].IsClosed)
            {
                return false;
            }

            WaitHandle.SignalAndWait(this._mismatchWaitHandles[index], this._inputReaderWaitHandles[index]);
            return !this._inputReaders[index].IsClosed;
        }

        private int CompareDataRecords()
        {
            if (this._inputReaders[0].IsClosed)
            {
                return 1;
            }

            if (this._inputReaders[1].IsClosed)
            {
                return -1;
            }

            return this._comparer.Compare(this._inputReaders[0], this._inputReaders[1]);
        }

        private void readInputReader(int index)
        {
            if (!this._inputReaders[index].Read())
            {
                this._inputReaders[index].Close();
                if (this._inputReaderWaitHandles[index] != null)
                {
                    this._inputReaderWaitHandles[index].Set();
                }
            }
            else
            {
                if (index == 0)
                {
                    this.NumberOfRecordsReadFromSource1++;
                }
                else
                {
                    this.NumberOfRecordsReadFromSource2++;
                }
            }
        }

        #endregion Methods
    }
}
