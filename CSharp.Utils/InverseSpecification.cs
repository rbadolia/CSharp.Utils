using CSharp.Utils.Contracts;
using CSharp.Utils.Validation;

namespace CSharp.Utils
{
    public class InverseSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _decoratedSpecification;

        public InverseSpecification(ISpecification<T> specificationToBeDecorated)
        {
            Guard.ArgumentNotNull(specificationToBeDecorated, "specificationToBeDecorated");
            this._decoratedSpecification = specificationToBeDecorated;
        }

        public ISpecification<T> DecoratedSpecification
        {
            get { return this._decoratedSpecification; }
        }

        public bool IsSatisfiedBy(T obj)
        {
            return !this._decoratedSpecification.IsSatisfiedBy(obj);
        }
    }
}
