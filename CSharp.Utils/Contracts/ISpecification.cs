namespace CSharp.Utils.Contracts
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T obj);
    }
}
