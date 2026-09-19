using System;

namespace Hp2BaseMod; 

public class GameStateTitle : IGameState 
{ 
    public RelativeId Id => GameStateId.Title; 
    
    public bool IsSavable => false;

    public void Enter() 
    { 
        ModInterface.Events.GameSessionBegan += OnGameSessionBegan;
    }
    
    public void Exit() 
    {
        ModInterface.Events.GameSessionBegan -= OnGameSessionBegan; 
    }
    
    private void OnGameSessionBegan(GameSession gameSession) 
    { 
        var targetStateId = ModInterface.Save.GetCurrentFile().GameStateId ?? GameStateId.Hub; 
        ModInterface.GameState.ChangeState(targetStateId);
    }
    
    public string GetProfileString(PlayerFile playerFile) => string.Empty;

    public void DepartTransition(RelativeId? nextStateId, Action continueTransitionCallback)
    {
        throw new NotImplementedException();
    }

    public void DepartTransition(Action continueTransitionCallback)
    {
        throw new NotImplementedException();
    }
}
