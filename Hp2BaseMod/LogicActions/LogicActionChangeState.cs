namespace Hp2BaseMod;

public class LogicActionChangeState(RelativeId targetState) : LogicAction, IFunctionalLogicAction
{
    private RelativeId _targetState = targetState;
    
    public void Act() 
    { 
        Game.Session.Location.GetExpansion().PreLocationArrive += On_PreLocationArrive;
    }

    private void On_PreLocationArrive(LocationArriveArgs args)
    {
        Game.Session.Location.GetExpansion().PreLocationArrive -= On_PreLocationArrive;
        ModInterface.Log.Message($"Changing to GameState {_targetState} via Logic Action.");
        ModInterface.GameState.ChangeState(_targetState);
    }
}
