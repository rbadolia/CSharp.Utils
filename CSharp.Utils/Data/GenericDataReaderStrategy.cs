using System;
using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public delegate T ReadNextCallback<out T>();

    public delegate void PopulateItemArrayDelegate<in T>(T obj, object[] itemArray);

    public delegate void DisposeDelegate();

    public class GenericDataReaderStrategy<T> : AbstractDataReaderStrategy<T>
        where T : class
    {
        #region Fields

        private readonly IList<KeyValuePair<string, Type>> _columns;

        private readonly DisposeDelegate _disposeCallback;

        private readonly PopulateItemArrayDelegate<T> _populateItemArrayDelegate;

        private readonly ReadNextCallback<T> _readNextCallback;

        #endregion Fields

        #region Constructors and Finalizers

        public GenericDataReaderStrategy(IList<KeyValuePair<string, Type>> columns, ReadNextCallback<T> readNextCallback, PopulateItemArrayDelegate<T> populateItemArrayDelegate, DisposeDelegate disposeCallback)
        {
            this._columns = columns;
            this._readNextCallback = readNextCallback;
            this._populateItemArrayDelegate = populateItemArrayDelegate;
            this._disposeCallback = disposeCallback;
        }

        #endregion Constructors and Finalizers

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this._disposeCallback != null)
            {
                this._disposeCallback();
            }
        }

        protected override IList<KeyValuePair<string, Type>> getColumns()
        {
            return this._columns;
        }

        protected override void populateItemArray(T obj, object[] itemArray)
        {
            this._populateItemArrayDelegate(obj, itemArray);
        }

        protected override T readNextObject()
        {
            return this._readNextCallback();
        }

        #endregion Methods
    }
}
