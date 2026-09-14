using System.Collections.Generic;

namespace Hp2BaseMod;

public class GameStateManager
{
    private readonly Dictionary<RelativeId, IGameState> _states = new();
    private IGameState _currentState;

    public IGameState CurrentState => _currentState;

    public bool AllowPause => _currentState?.AllowPause ?? true;
    public bool AllowSave => _currentState?.AllowSave ?? true;
    public bool UseLeftCellphone => _currentState?.UseLeftCellphone ?? false;

    /// <summary>
    /// Registers a game state handler.
    /// </summary>
    public void RegisterState(IGameState state)
    {
        if (state == null) return;
        _states[state.Id] = state;
    }

    /// <summary>
    /// Resolves a state by ID.
    /// </summary>
    public IGameState GetState(RelativeId stateId)
    {
        return _states.TryGetValue(stateId, out var state) ? state : null;
    }

    /// <summary>
    /// Switches the active state to the given state instance or ID.
    /// </summary>
    public void ChangeState(RelativeId stateId)
    {
        var state = GetState(stateId);
        if (state == null)
        {
            ModInterface.Log.Error($"Attempted to transition to unregistered GameState '{stateId}'.");
            return;
        }
        ChangeState(state);
    }

    /// <summary>
    /// Switches the active state to the given state instance or ID.
    /// </summary>
    public void ChangeState(IGameState nextState)
    {
        if (nextState == null || _currentState == nextState) return;

        var previousState = _currentState;
        ModInterface.Log.Message($"GameState Transition: {previousState?.Id.ToString() ?? "NONE"} -> {nextState.Id}");

        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }

    public void NotifyLocationArrive(LocationArriveArgs args)
    {
        _currentState?.OnLocationArrive(args);
    }

    public void NotifyLocationSettled(LocationSettledArgs args)
    {
        _currentState?.OnLocationSettled(args);
    }

    public void ResetDolls(bool unload = false)
    {
        _currentState?.ResetDolls(unload);
    }
}