using System;

namespace CSharp.Utils.Linq
{
    [Serializable]
    public sealed class ParseException : Exception
    {
        #region Fields

        private readonly int position;

        #endregion Fields

        #region Constructors

        public ParseException(string message, int position)
            : base(message)
        {
            this.position = position;
        }

        #endregion Constructors

        #region Properties

        public int Position
        {
            get { return position; }
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return string.Format(Res.ParseExceptionFormat, Message, position);
        }

        #endregion Methods
    }
}
