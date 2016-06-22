using System.Collections.Generic;

namespace CSharp.Utils.Collections.Generic
{
    public class CompositeFilter<T> : IFilter<T>
    {
        public CompositeFilter()
        {
            this.InnerFilters = new List<IFilter<T>>();
        }

        public List<IFilter<T>> InnerFilters
        {
            get; set;
        }

        public bool ShouldFilter(T obj)
        {
            foreach (var filter in this.InnerFilters)
            {
                if (filter.ShouldFilter(obj))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
