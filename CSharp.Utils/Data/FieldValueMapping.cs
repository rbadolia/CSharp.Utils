using System;

namespace CSharp.Utils.Data
{
    [Serializable]
    public sealed class FieldValueMapping
    {
        #region Public Properties

        public string FieldName { get; set; }

        public string FieldValue { get; set; }

        public string MappedValue { get; set; }

        #endregion Public Properties
    }
}
