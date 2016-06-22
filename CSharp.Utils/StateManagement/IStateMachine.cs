using System.Collections.Generic;

namespace CSharp.Utils.StateManagement
{
    public interface IStateMachine<TState, TAction>
    {
        bool CanPerformAction(TAction action);

        IEnumerable<TAction> GetPermittedActions();

        IReadOnlyCollection<TState> AllStates { get; }

        IReadOnlyCollection<TAction> AllActions { get; }

        string GetDotGraph(bool excludeActionsWithNoTransition);
    }
}
