using System.Text;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Text
{
    public class SelfFormattingStringBuilder
    {
        #region Fields

        private Pair<char, char>[] _braces;

        private StringBuilder _sb = new StringBuilder();

        #endregion Fields

        #region Constructors and Finalizers

        public SelfFormattingStringBuilder(params Pair<char, char>[] braces)
        {
            this._braces = braces;
        }

        #endregion Constructors and Finalizers
    }
}
