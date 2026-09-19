using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

public class GameStateManager
{
    /// <summary>
    /// Notifies when the game state changes
    /// </summary>
    public event Action<(RelativeId? prevStateId, RelativeId currentStateId)> GameStateChanged;

    /// <summary>
    /// The game state currently running
    /// </summary>
    public IGameState CurrentState => _currentState;
    private IGameState _currentState;

    private Dictionary<RelativeId, IGameState> _states;

    public void Init(Dictionary<RelativeId, IGameState> states)
    {
        _states = states;
    }

    /// <summary>
    /// Switches the active state to the given Id.
    /// </summary>
    public void ChangeState(RelativeId stateId)
    {
        if (!_states.TryGetValue(stateId, out var state))
        {
            ModInterface.Log.Error($"Attempted to transition to unregistered GameState '{stateId}'.");
            return;
        }
        ChangeState(state);
    }

    /// <summary>
    /// Switches the active state to the given state instance.
    /// </summary>
    public void ChangeState(IGameState nextState)
    {
        if (nextState == null || _currentState == nextState) return;

        var previousStateId = _currentState?.Id;
        ModInterface.Log.Message($"GameState Transition: {previousStateId.ToString() ?? "NONE"} -> {nextState.Id}");

        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
        GameStateChanged?.Invoke((previousStateId, _currentState.Id));
    }
}
