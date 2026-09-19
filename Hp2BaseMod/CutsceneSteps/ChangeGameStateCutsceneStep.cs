using System;
using Hp2BaseMod.ModGameData.Interface; 

namespace Hp2BaseMod.ModGameData; 

/// <summary> 
/// A functional cutscene step that updates the active GameState 
/// Supports explicit GameState RelativeIds or dynamic resolution from a LocationDefinition.
/// </summary> 
public class ChangeGameStateCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep 
{
    public event CutsceneStepComplete Complete;
    private readonly RelativeId _targetStateId;

    public ChangeGameStateCutsceneStep(RelativeId targetStateId) 
    { 
        _targetStateId = targetStateId; 
    }
    
    public void Act() 
    { 
        Game.Session.Location.GetExpansion().PreLocationArrive += On_PreLocationArrive;
        Complete?.Invoke(this);
    }

    private void On_PreLocationArrive(LocationArriveArgs args)
    {
        Game.Session.Location.GetExpansion().PreLocationArrive -= On_PreLocationArrive;
        ModInterface.Log.Message($"Changing to GameState {_targetStateId} via cutscene step.");
        ModInterface.GameState.ChangeState(_targetStateId);
    }
}
