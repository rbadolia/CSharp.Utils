namespace CSharp.Utils.Threading
{
    public interface IDispatcherDependent
    {
        IDispatcher GetDispatcher();
    }
}
