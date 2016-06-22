using System.Collections.Generic;
using System.Reflection;

namespace CSharp.Utils.Data.Dynamic
{
    public static partial class DynamicDAO<E>
    {
        private sealed class MetaData
        {
            #region Constructors and Finalizers

            public MetaData()
            {
                this.KeyProperties = new List<KeyValuePair<PropertyInfo, IDbColumnMetadata>>();
                this.NonKeyProperties = new List<KeyValuePair<PropertyInfo, IDbColumnMetadata>>();
            }

            #endregion Constructors and Finalizers

            #region Public Properties

            public List<KeyValuePair<PropertyInfo, IDbColumnMetadata>> KeyProperties { get; private set; }

            public List<KeyValuePair<PropertyInfo, IDbColumnMetadata>> NonKeyProperties { get; private set; }

            #endregion Public Properties
        }
    }
}
