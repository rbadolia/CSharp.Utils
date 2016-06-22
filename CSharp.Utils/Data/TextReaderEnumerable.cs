using System.Collections;
using System.Collections.Generic;
using System.IO;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data
{
    public class TextReaderEnumerable : IEnumerable<string>
    {
        #region Fields

        private readonly TextReader _reader;

        #endregion Fields

        #region Constructors and Finalizers

        public TextReaderEnumerable(TextReader reader)
        {
            Guard.ArgumentNotNull(reader, "reader");
            this._reader = reader;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public IEnumerator<string> GetEnumerator()
        {
            return new TextReaderEnumerator(this._reader);
        }

        #endregion Public Methods and Operators

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TextReaderEnumerator(this._reader);
        }

        #endregion Explicit Interface Methods
    }
}
