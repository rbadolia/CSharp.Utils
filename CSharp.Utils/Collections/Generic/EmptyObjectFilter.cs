using CSharp.Utils.Extensions;

namespace CSharp.Utils.Collections.Generic
{
    public class EmptyObjectFilter<T>:IFilter<T>
    {
        public bool ShouldFilter(T obj)
        {
            return !obj.DynamicIsEmpty(false);

        }
    }
}
