using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace CSharp.Utils.Data
{

    #region Delegates

    public delegate T CreateEntityFromReaderDelegate<out T>(IDataRecord record);

    #endregion Delegates

    public class DataReaderEnumerable<T> : IEnumerable<T>
        where T : class
    {
        #region Fields

        private readonly bool closeConnectionAfterReading;

        private readonly IDbConnection connection;

        private readonly CreateEntityFromReaderDelegate<T> createEntityFromReaderDelegate;

        private readonly IDataReader reader;

        #endregion Fields

        #region Constructors and Finalizers

        public DataReaderEnumerable(IDataReader reader, CreateEntityFromReaderDelegate<T> createEntityFromReaderDelegate, IDbConnection connection, bool closeConnectionAfterReading)
        {
            this.reader = reader;
            this.connection = connection;
            this.createEntityFromReaderDelegate = createEntityFromReaderDelegate;
            this.closeConnectionAfterReading = closeConnectionAfterReading;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<T> GetEnumerator()
        {
            return new DataReaderEnumerator(this.reader, this.createEntityFromReaderDelegate, this.connection, this.closeConnectionAfterReading);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataReaderEnumerator(this.reader, this.createEntityFromReaderDelegate, this.connection, this.closeConnectionAfterReading);
        }

        #endregion Explicit Interface Methods

        private sealed class DataReaderEnumerator : IEnumerator<T>
        {
            #region Fields

            private readonly bool _closeConnectionAfterReading;

            private readonly IDbConnection _connection;

            private readonly CreateEntityFromReaderDelegate<T> _createEntityFromReaderDelegate;

            private readonly IDataReader _reader;

            #endregion Fields

            #region Constructors and Finalizers

            public DataReaderEnumerator(IDataReader reader, CreateEntityFromReaderDelegate<T> createEntityFromReaderDelegate, IDbConnection connection, bool closeConnectionAfterReading)
            {
                this._reader = reader;
                this._connection = connection;
                this._createEntityFromReaderDelegate = createEntityFromReaderDelegate;
                this._closeConnectionAfterReading = closeConnectionAfterReading;
            }

            #endregion Constructors and Finalizers

            #region Explicit Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    return this._createEntityFromReaderDelegate(this._reader);
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    return this._createEntityFromReaderDelegate(this._reader);
                }
            }

            #endregion Explicit Interface Properties

            #region Public Methods and Operators

            public void Dispose()
            {
                if (this._closeConnectionAfterReading)
                {
                    try
                    {
                        this._connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            public bool MoveNext()
            {
                if (this._reader.Read())
                {
                    return true;
                }

                if (this._closeConnectionAfterReading)
                {
                    this._connection.Close();
                }

                return false;
            }

            public void Reset()
            {
            }

            #endregion Public Methods and Operators
        }
    }
}
