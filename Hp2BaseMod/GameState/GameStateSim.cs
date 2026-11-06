using System;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Active while the player is with a pair at a location where they talk, give gifts and ask for a date.
/// </summary>
public sealed class GameStateSim : GameStateLocation
{
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new GameStateSim();

        public string GetProfileString(PlayerFile playerFile)
        {
            return playerFile.girlPairDefinition?.GetExpansion().GetNames(playerFile.sidesFlipped) ?? "Alone";
        }

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            if (ModInterface.Data.TryGetDataId(GameDataType.GirlPair, saveFile.girlPairId, out var pairId) 
                && ModInterface.GameData.GetGirlPair(pairId) == null)
            {
                error = "Sim state requires a valid girlPair";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public class Payload(RelativeId pairId) : IGameStatePayload
    {
        public RelativeId PairId = pairId;
    }

    public override bool AutosavesOnArrival => true;

    public override RelativeId Id => GameStateId.Sim;

    public override bool IsSavable => true;

    // ---- what the player can use here ----

    public override bool HidesCellphone(UiCellphone cellphone) => false;

    // the slots are ready once the talk and date bubbles are showing
    public override FinderStatus GetFinderStatus()
        => FinderStatus.Present(Game.Manager.Windows.IsWindowActive(Game.Session.Location.actionBubblesWindow, true, false), 0);

    // picking a place just means leaving for it
    public override bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot) => false;

    public override bool AllowsStore => true;
    public override bool StoreShowsNotice => false;
    public override bool AllowsPairApp => true;
    public override bool HasGirlsPresent => true;
    public override bool ShowsHubMenu => false;
    public override bool UseStylesOnDatesFlag => false;

    public override string GetGiftError(ItemDefinition item)
    {
        if (Game.Session.Location.currentGirlPair == null) return NoGirlsNearbyGiftError;
        if (!Game.Manager.Windows.IsWindowActive(Game.Session.Location.actionBubblesWindow, true, false)) return TalkingGiftError;
        return null;
    }

    public override bool DialogIgnoresCellphone => false;

    // gifts can go to either girl, and nobody is the focused one
    public override bool DollFocusIsInteractive => false;

    public override bool HintsSimStaminaCost => true;
    public override bool AllowsPause => true;

    private bool _disposed;
    public GameStateSim() 
        : base()
    {
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PostLocationSettled += OnPostLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.LocationArrived += On_LocationArrived;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
        ModInterface.Events.DateLocationSelected += On_DateLocationSelected;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            _pendingState = null;
            var locationManager = Game.Session.Location.GetExpansion();
            locationManager.PostLocationSettled -= OnPostLocationSettled;
            locationManager.ResolveDollStyles -= OnResolveDollStyles;
            locationManager.LocationArrived -= On_LocationArrived;

            ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;
            ModInterface.Events.DateLocationSelected -= On_DateLocationSelected;
        }

        base.Dispose(disposing);
    }

    private void OnPostLocationSettled(LocationSettledArgs args)
    {
        Game.Manager.Windows.ShowWindow(args.actionBubblesWindow ?? Game.Session.Location.actionBubblesWindow, false);

        if (args.hasArrivalCutscene) return;

        var randomDollArgs = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(randomDollArgs);

        var doll = randomDollArgs.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());
        var dtGreetings = Game.Session.Location.dtGreetings;
        var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, dtGreetings.Length - 1);

        doll.ReadDialogTrigger(dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);
        ModInterface.Log.Message("GameState.Sim: settled - greeted player");
    }

    private void OnResolveDollStyles(ResolveDollStylesArgs args) => ApplyDefaultDollStyles(args);

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        // I'd like a better way to check this. Maybe I could add
        // a flag to the hub slot itself?
        if (slot.locationDefinition?.ModId() == Locations.HotelRoom)
        {
            _pendingState = new GameStateHub.Payload();
        }
        else
        {
            _pendingState = new Payload(slot.playerFileFinderSlot.girlPairDefinition.ModId());
        }
    }

    private void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        _pendingState = new GameStatePuzzle.Payload(PuzzleStateId.Normal);
    }

    private Action _continueTransitionCallback;
    private UiDoll _targetDoll;

    public override void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        if (payload != null)
        {
            _pendingState = payload;
        }

        //TODO, see about moving this to the states?
        //I don't like checking the type of the pending state here
        //this didn't work...
        var dialogTriggerDefinition = _pendingState?.GetType() == typeof(GameStatePuzzle.Payload)
            ? Game.Session.Location.dtAskDate
            : Game.Session.Location.dtValediction;

        if (dialogTriggerDefinition != null)
        {
            var args = new RandomDollSelectedArgs();
            ModInterface.Events.NotifyRandomDollSelected(args);
            _targetDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

            _continueTransitionCallback = continueTransitionCallback;
            _targetDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
            _targetDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
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
        var locationManager = Game.Session.Location;

        if (args.girlPairDef != null)
        {
            // there is no puzzle state to ask here, so the status is built with a normal date's rules
            using var rules = new PuzzleStateNormal(this);
            Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(rules, locationManager.currentGirlLeft, locationManager.currentGirlRight);
            Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.staminaFreeze = -1;
            Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.staminaFreeze = -1;
        }
        else
        {
            Game.Session.Puzzle.puzzleStatus.Clear();
        }

        FinishArrival(args, processArriveBundles: false); // TODO: confirm Sim intentionally skips arrive bundles

        if (args.girlPairDef != null && !args.girlPairDef.specialPair)
        {
            var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(args.girlPairDef.girlDefinitionOne);
            playerFileGirl.playerMet = true;

            var playerFileGirl2 = Game.Persistence.playerFile.GetPlayerFileGirl(args.girlPairDef.girlDefinitionTwo);
            playerFileGirl2.playerMet = true;

            var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(args.girlPairDef);
            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                playerFileGirlPair.RelationshipLevelUp();
                // Now actually reaches the transition (previously written to a
                // LocationArriveGooArgs field nothing read back) - see the
                // ExpandedLocationManager.Arrive_Prefix write-back after this fires.
                args.arrivalCutscene = args.girlPairDef.introductionPair
                    ? locationManager.cutsceneMeetingIntro
                    : locationManager.cutsceneMeeting;
            }
        }
    }

    public override IGameStatePayload GeneratePayload() => new Payload(Game.Persistence.playerFile.girlPairDefinition.ModId());
}