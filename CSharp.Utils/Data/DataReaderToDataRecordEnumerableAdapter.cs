using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CSharp.Utils.Data
{
    public sealed class DataReaderToDataRecordEnumerableAdapter : IEnumerable<IDataRecord>
    {
        #region Fields

        private readonly IDataReader _reader;

        #endregion Fields

        #region Constructors and Finalizers

        public DataReaderToDataRecordEnumerableAdapter(IDataReader reader)
        {
            this._reader = reader;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<IDataRecord> GetEnumerator()
        {
            return new DataReaderToDataRecordEnumeratorAdapter(this._reader);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion Explicit Interface Methods

        private sealed class DataReaderToDataRecordEnumeratorAdapter : IEnumerator<IDataRecord>
        {
            #region Fields

            private readonly IDataReader reader;

            #endregion Fields

            #region Constructors and Finalizers

            public DataReaderToDataRecordEnumeratorAdapter(IDataReader reader)
            {
                this.reader = reader;
            }

            #endregion Constructors and Finalizers

            #region Public Properties

            public IDataRecord Current
            {
                get
                {
                    return this.reader;
                }
            }

            #endregion Public Properties

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this.reader;
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return this.reader.Read();
            }

            public void Reset()
            {
            }

            #endregion Public Methods and Operators
        }
    }
}
