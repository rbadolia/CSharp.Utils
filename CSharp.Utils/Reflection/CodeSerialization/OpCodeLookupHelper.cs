using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    internal static class OpCodeLookupHelper
    {
        #region Static Fields

        private static readonly Dictionary<short, OpCode> _opCodesDictionary;

        #endregion Static Fields

        #region Constructors and Finalizers

        static OpCodeLookupHelper()
        {
            FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
            _opCodesDictionary = fields.Select(field => (OpCode)field.GetValue(null)).ToDictionary(code => code.Value);
        }

        #endregion Constructors and Finalizers

        #region Methods

        internal static OpCode GetOpCodeByValue(short value)
        {
            return _opCodesDictionary[value];
        }

        internal static bool TryGetOpCodeByValue(short value, out OpCode code)
        {
            return _opCodesDictionary.TryGetValue(value, out code);
        }

        #endregion Methods
    }
}
