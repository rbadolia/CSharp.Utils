namespace CSharp.Utils.Contracts
{
    public interface IConcurrencySupported
    {
        byte[] RowVersion
        {
            get;
            set;
        }
    }
}
