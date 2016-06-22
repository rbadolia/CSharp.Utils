namespace CSharp.Utils.Contracts
{
    public interface IStateSynchronizationSupported<TState>
    {
        void SynchronizeFromState(TState parcelState);
    }
}
