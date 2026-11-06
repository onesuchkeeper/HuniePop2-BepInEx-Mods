using System;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// Active at the hub. The hub has no girls to be with: the only girl is the one who runs it.
/// </summary>
public sealed class GameStateHub : GameStateLocation
{
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new GameStateHub();

        public string GetProfileString(PlayerFile playerFile) => "Kyu";

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public class Payload : IGameStatePayload
    {
        
    }

    public override RelativeId Id => GameStateId.Hub;
    public override bool IsSavable => true;

    // ---- what the player can use here ----

    // The phone's own setting decides.
    public override bool HidesCellphone(UiCellphone cellphone) => cellphone.hideOnHub;

    /// <summary>
    /// The finder is always there. Its slots are ready while the root menu is showing, or while the player still has to pick
    /// the first location (the finder is how they do it).
    /// </summary>
    public override FinderStatus GetFinderStatus()
    {
        var rootMenuShowing = Game.Session.Hub.hubStepType == HubStepType.ROOT
            && Game.Manager.Windows.IsWindowActive(Game.Session.Dialog.dialogOptionsWindow, true, false);

        var firstLocationPending = Game.Persistence.playerFile.GetFlagValue(Game.Session.Hub.firstLocationFlag) < 0;

        return FinderStatus.Present(rootMenuShowing || firstLocationPending, 2);
    }

    /// <summary>
    /// The first location pick is only recorded: the flag is set to the place and the phone closes, and the player stays.
    /// Every pick after that has the player leave, after the hub has been wound down.
    /// </summary>
    public override bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot)
    {
        if (Game.Persistence.playerFile.GetFlagValue(Game.Session.Hub.firstLocationFlag) < 0)
        {
            Game.Persistence.playerFile.SetFlagValue(Game.Session.Hub.firstLocationFlag, slot.locationDefinition.id);
            app.cellphone.Close();
            return true;
        }

        Game.Session.Hub.BailHub();
        return false;
    }

    public override bool AllowsStore => true;
    public override bool StoreShowsNotice => false;
    public override bool AllowsPairApp => true;
    public override bool HasGirlsPresent => false;

    public override string GetGiftError(ItemDefinition item) => NoGirlsNearbyGiftError;

    // the hub girl talks beside the phone as much as without it
    public override bool DialogIgnoresCellphone => true;

    public override bool DollFocusIsInteractive => false;
    public override bool HintsSimStaminaCost => false;

    // the menus can only be opened from the root menu
    public override bool AutosavesOnArrival => true;

    public override bool AllowsPause => Game.Session.Hub.hubStepType == HubStepType.ROOT;

    public override bool ShowsHubMenu => true;

    public override bool UseStylesOnDatesFlag => false;

    public GameStateHub() 
        : base()
    {
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationArrive += On_PreLocationArrive;
        locationManager.LocationArrived += On_LocationArrived;
        locationManager.PreLocationSettled += OnPreLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;

        Game.Session.Hub.GetExpansion().StateDeparting += On_StateDeparting;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
    }

    protected override void Dispose(bool disposing)
    {
        _pendingState = null;
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationArrive -= On_PreLocationArrive;
        locationManager.LocationArrived -= On_LocationArrived;
        locationManager.PreLocationSettled -= OnPreLocationSettled;
        locationManager.ResolveDollStyles -= OnResolveDollStyles;

        Game.Session.Hub.GetExpansion().StateDeparting -= On_StateDeparting;

        ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;

        base.Dispose(disposing);
    }

    private void On_PreLocationArrive(LocationArriveArgs args)
    {
        args.cellphoneOnLeft = true;
    }

    private void OnPreLocationSettled(LocationSettledArgs args)
    {
        ModInterface.Log.Message("Hub pre location settled");
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

        // the hub girl is the only girl, on the right
        Game.Session.gameCanvas.dollLeft.UnloadGirl();
        Game.Session.gameCanvas.dollRight.LoadGirl(hubDef, -1, -1, -1, null);

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

    private void On_StateDeparting(IGameStatePayload id)
    {
        _pendingState = id;
    }

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        _pendingState = new GameStateSim.Payload(slot.playerFileFinderSlot.girlPairDefinition.ModId());
    }

    private Action _continueTransitionCallback;
    private UiDoll _targetDoll;

    public override void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        if (payload != null)
        {
            _pendingState = payload;
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

    public override IGameStatePayload GeneratePayload() => new Payload();
}