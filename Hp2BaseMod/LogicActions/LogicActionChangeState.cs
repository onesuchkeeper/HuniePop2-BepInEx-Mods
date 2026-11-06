namespace Hp2BaseMod;

/// <summary>
/// Changes game state as part of a logic bundle or cutscene. Bundles run it just before the TRAVEL_TO it was generated for.
/// The change is queued on the current state, which the location manager applies when the travel arrives: after the departure and
/// before PreLocationArrive is raised, so the new state is already active (and has subscribed to its events) for the whole arrival.
/// It is safe to run more than once before an arrival: the last queued state wins.
/// </summary>
public class LogicActionChangeState : LogicAction, IFunctionalLogicAction
{
    private readonly IGameStatePayload _payload;

    public LogicActionChangeState(IGameStatePayload payload)
    {
        _payload = payload;
        type = (LogicActionType)(-1);
    }

    public void Act()
    {
        if (ModInterface.GameState.CurrentState is GameStateLocation locationState)
        {
            ModInterface.Log.Message($"Queueing transition to GameState {_payload} via Logic Action.");
            locationState.QueueTransition(_payload);
            return;
        }

        // There is no arrival to wait for from a state that is not a location state (the title screen, for example).
        ModInterface.Log.Message($"Changing to GameState {_payload} via Logic Action (the current state is not a location state).");
        ModInterface.GameState.ChangeState(_payload);
    }
}