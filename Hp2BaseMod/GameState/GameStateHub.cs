using System;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// Active for LocationType.HUB locations.
/// </summary>
public sealed class GameStateHub : GameStateLocation
{
    public override RelativeId Id => GameStateId.Hub;
    public override bool IsSavable => true;

    public override void Enter()
    {
        base.Enter();
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationArrive += On_PreLocationArrive;
        locationManager.LocationArrived += On_LocationArrived;
        locationManager.PreLocationSettled += OnPreLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;

        Game.Session.Hub.GetExpansion().StateDeparting += On_StateDeparting;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
    }

    public override void Exit()
    {
        base.Exit();
        _pendingState = null;
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationArrive -= On_PreLocationArrive;
        locationManager.LocationArrived -= On_LocationArrived;
        locationManager.PreLocationSettled -= OnPreLocationSettled;
        locationManager.ResolveDollStyles -= OnResolveDollStyles;

        Game.Session.Hub.GetExpansion().StateDeparting -= On_StateDeparting;

        ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;
    }

    public override string GetProfileString(PlayerFile playerFile) => "Kyu";

    private void On_PreLocationArrive(LocationArriveArgs args)
    {
        args.cellphoneOnLeft = true;
    }

    private void OnPreLocationSettled(LocationSettledArgs args)
    {
        if (!args.hasArrivalCutscene)
        {
            Game.Session.gameCanvas
                .GetDoll(DollOrientationType.RIGHT)
                .ReadDialogTrigger(Game.Session.Hub.GetGreeting(), DialogLineFormat.PASSIVE, -1);
        }

        ModInterface.Log.Message("GameState.Hub: settled - starting hub");
        Game.Session.Hub.StartHub();
    }

    private void OnResolveDollStyles(ResolveDollStylesArgs args)
    {
        var hubDef = Game.Session.Hub.hubGirlDefinition;
        var expansion = hubDef.GetExpansion();

        int outfitIndex = GetRandomValidIndex(hubDef.outfits);
        var outfit = hubDef.outfits[outfitIndex];

        // For the normal hub randomization, skip nsfw outfits entirely.
        if (outfit.GetExpansion().IsNSFW) return;

        var style = new GirlStyleInfo
        {
            OutfitId = expansion.OutfitLookup[outfitIndex],
            HairstyleId = outfit.pairHairstyleIndex != -1
                ? expansion.HairstyleLookup[outfit.pairHairstyleIndex]
                : expansion.HairstyleLookup[GetRandomValidIndex(hubDef.hairstyles)]
        };

        var requestArgs = ModInterface.Events.NotifyRequestStyleChange(hubDef, args.Location, 0.1f, style, false);

        if (ShouldApply(requestArgs.ApplyChance))
        {
            requestArgs.Style.Apply(
                Game.Session.gameCanvas.dollRight,
                hubDef.defaultOutfitIndex,
                hubDef.defaultHairstyleIndex);
        }
    }

    private void On_StateDeparting(RelativeId id)
    {
        _pendingState = id;
    }

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        _pendingState = GameStateId.Sim;
    }

    private Action _continueTransitionCallback;
    private UiDoll _targetDoll;

    public override void DepartTransition(RelativeId? _nextStateId, Action continueTransitionCallback)
    {
        if (_nextStateId.HasValue)
        {
            _pendingState = _nextStateId;
        }

        var dialogTriggerDefinition = Game.Session.Hub.GetValediction();

        if (dialogTriggerDefinition != null)
        {
            _continueTransitionCallback = continueTransitionCallback;
            _targetDoll = Game.Session.gameCanvas.GetDoll(DollOrientationType.RIGHT);
            _targetDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
            _targetDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
            return;
        }
        else
        {
            continueTransitionCallback();
        }
    }

    private void OnValedictionDialogRead(UiDoll doll)
    {
        if (_targetDoll == null)
        {
            ModInterface.Log.Warning("Valediction triggered with no target doll");
            return;
        }

        _targetDoll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        _continueTransitionCallback?.Invoke();
        _continueTransitionCallback = null;
    }

    private void On_LocationArrived(LocationArriveArgs args)
    {
        Game.Session.Puzzle.puzzleStatus.Clear();
        FinishArrival(args);
    }
}
