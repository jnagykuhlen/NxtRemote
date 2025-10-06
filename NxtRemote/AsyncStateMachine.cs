using System.Diagnostics;

namespace NxtRemote;

public class AsyncStateMachine<TState> where TState : struct, Enum
{
    private readonly Dictionary<TState, StateDescription> stateDescriptions = new(typeof(TState).GetEnumValues().Length);

    public AsyncStateMachine<TState> WithStateTransition(TState state, Func<Task<TState>> onTransitionAsync) =>
        WithState(state, onTransitionAsync, false);
    
    public AsyncStateMachine<TState> WithStateTransition(TState state, Func<Task> onTransitionAsync, TState nextState) =>
        WithState(state, () => onTransitionAsync().ContinueWith(_ => nextState), false);
    
    public AsyncStateMachine<TState> WithFinalState(TState state) =>
        WithState(state, () => Task.FromResult(state), true);

    private AsyncStateMachine<TState> WithState(TState state, Func<Task<TState>> onTransitionAsync, bool isFinal)
    {
        if (!stateDescriptions.TryAdd(state, new StateDescription(onTransitionAsync, isFinal)))
            throw new InvalidOperationException($"State '{state}' is already defined.");

        return this;
    }
    
    public async Task<TState> RunAsync(TState startState, TimeSpan timeout)
    {
        var currentState = startState;
        var targetTime = DateTime.Now.Add(timeout);
        
        while (DateTime.Now < targetTime)
        {
            if (!stateDescriptions.TryGetValue(currentState, out var stateDescription))
                throw new InvalidOperationException($"State '{currentState}' is not defined.");
                
            if (stateDescription.IsFinal)
                return currentState;

            currentState = await stateDescription.TransitionAsync();
        }

        return currentState;
    }

    private class StateDescription(Func<Task<TState>> onTransitionAsync, bool isFinal)
    {
        public Task<TState> TransitionAsync() => onTransitionAsync();
        public bool IsFinal { get; } = isFinal;
    }
}
