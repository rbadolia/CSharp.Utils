using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CSharp.Utils.Data
{
    public class TextReaderEnumerator : IEnumerator<string>
    {
        #region Fields

        private readonly TextReader _reader;

        #endregion Fields

        #region Constructors and Finalizers

        public TextReaderEnumerator(TextReader reader)
        {
            this._reader = reader;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string Current { get; private set; }

        #endregion Public Properties

        #region Explicit Interface Properties

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        #endregion Explicit Interface Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            this._reader.Dispose();
        }

        public bool MoveNext()
        {
            if (this._reader.Peek() != -1)
            {
                string line = this._reader.ReadLine();
                this.Current = string.IsNullOrEmpty(line) ? null : line;
                return true;
            }

            this._reader.Close();
            this.Current = null;
            return false;
        }

        public void Reset()
        {
        }

        #endregion Public Methods and Operators
    }
}
