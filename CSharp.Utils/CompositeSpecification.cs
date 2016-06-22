using System.Collections.Generic;
using CSharp.Utils.Contracts;

namespace CSharp.Utils
{
    public class CompositeSpecification<T>:ISpecification<T>
    {
        private readonly IEnumerable<ISpecification<T>> _specifications;

        public CompositeSpecification(IEnumerable<ISpecification<T>> specifications)
        {
            this._specifications = specifications;
        }

        public bool IsSatisfiedBy(T obj)
        {
            foreach (var specification in this._specifications)
            {
                if (!specification.IsSatisfiedBy(obj))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
