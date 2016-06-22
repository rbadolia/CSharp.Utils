using System;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public class PredicateBasedFilter<T> : IFilter<T>
    {
        private readonly Predicate<T> predicate;

        public PredicateBasedFilter(Predicate<T> predicate)
        {
            Guard.ArgumentNotNull(predicate, "predicate");
            this.predicate = predicate;
        }

        public bool ShouldFilter(T obj)
        {
            return this.predicate(obj);
        }
    }
}
