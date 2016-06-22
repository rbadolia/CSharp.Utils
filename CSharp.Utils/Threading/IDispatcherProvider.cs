namespace CSharp.Utils.Threading
{
    public interface IDispatcherProvider
    {
        IDispatcher GetDispatcher(object target);
    }
}
