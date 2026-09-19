using System;

namespace Hp2BaseMod;

/// <summary>
/// Active for LocationType.SPECIAL locations.
/// </summary>
public sealed class GameStateSpecial : GameStateLocation
{
    public override RelativeId Id => GameStateId.Special;

    public override bool IsSavable => true;

    public override void Enter()
    {
        base.Enter();
        var locationManager = Game.Session.Location.GetExpansion(); 
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.LocationArrived += On_LocationArrived;
    }

    public override void Exit()
    {
        base.Exit();
        var locationManager = Game.Session.Location.GetExpansion(); 
        locationManager.ResolveDollStyles -= OnResolveDollStyles;
        locationManager.LocationArrived -= On_LocationArrived;
    }

    public override string GetProfileString(PlayerFile playerFile) =>
        Game.Persistence.playerFile.locationDefinition != null
            ? Game.Persistence.playerFile.locationDefinition.locationName
            : "Special Event";

    private void OnResolveDollStyles(ResolveDollStylesArgs args) => ApplyDefaultDollStyles(args);

    public override void DepartTransition(RelativeId? nextStateId, Action continueTransitionCallback)
    {
        if (nextStateId.HasValue)
        {
            _pendingState = nextStateId;
        }
        
        continueTransitionCallback();
    }

    private void On_LocationArrived(LocationArriveArgs args)
    {
        Game.Session.Puzzle.puzzleStatus.Clear();
        FinishArrival(args);
    }
}