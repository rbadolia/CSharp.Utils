namespace CSharp.Utils.Contracts
{
    public interface IStateExportable<TState>
    {
        TState ExportState();
    }
}
