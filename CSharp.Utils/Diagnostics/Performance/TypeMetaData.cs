using System;
using System.Collections.Generic;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class TypeMetaData
    {
        #region Constructors and Finalizers

        public TypeMetaData()
        {
            this.Properties = new List<PropertyMetaData>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryHelp { get; set; }

        public string CategoryName { get; set; }

        public List<PropertyMetaData> Properties { get; private set; }

        public Type Type { get; set; }

        #endregion Public Properties
    }
}
