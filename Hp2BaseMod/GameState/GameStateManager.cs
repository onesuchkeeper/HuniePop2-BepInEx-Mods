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

    /// <summary>
    /// Switches the active state to the given state instance.
    /// </summary>
    public void ChangeState(IGameStatePayload payload)
    {
        var args = new ModEvents.ResolveStatePayloadArgs() 
        { 
            Payload = payload 
        };

        ModInterface.Events.NotifyResolveStatePayload(args);

        payload = args.Payload;

        if (payload == null) return;

        var prevStateId = _currentState?.Id;
        var previousStateName = prevStateId?.ToString() ?? "NONE";
        _currentState?.Dispose();

        _currentState = ModInterface.GameData.GameStates.Create(payload);

        if (_currentState == null)
        {
            ModInterface.Log.Error("Failed to create state, defaulting to hub");
            _currentState = ModInterface.GameData.GameStates.Create(new GameStateHub.Payload());

            if (_currentState == null) throw new Exception("Failed to create hub state!");
        }

        ModInterface.Log.Message($"GameState Transition: {previousStateName} -> {_currentState.Id}");

        GameStateChanged?.Invoke((prevStateId, _currentState.Id));
    }
}
