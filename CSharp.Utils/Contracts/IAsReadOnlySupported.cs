namespace CSharp.Utils.Contracts
{
    public interface IAsReadOnlySupported<out T>
    {
        T AsReadOnly();

        bool IsReadOnly();
    }
}
