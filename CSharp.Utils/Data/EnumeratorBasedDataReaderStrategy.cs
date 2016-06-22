using System;
using System.Collections.Generic;

namespace CSharp.Utils.Data
{
    public class EnumeratorBasedDataReaderStrategy<T> : GenericDataReaderStrategy<T>
        where T : class
    {
        #region Constructors and Finalizers

        public EnumeratorBasedDataReaderStrategy(IEnumerator<T> enumerator, IList<KeyValuePair<string, Type>> columns, PopulateItemArrayDelegate<T> populateItemArrayDelegate)
            : base(columns, delegate
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }

                    return default(T);
                }, populateItemArrayDelegate, null)
        {
        }

        #endregion Constructors and Finalizers
    }
}
