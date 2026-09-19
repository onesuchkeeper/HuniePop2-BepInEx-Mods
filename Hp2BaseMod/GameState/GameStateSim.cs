using System;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Active for LocationType.SIM locations.
/// </summary>
public sealed class GameStateSim : GameStateLocation
{
    public override RelativeId Id => GameStateId.Sim;

    public override bool IsSavable => true;

    public override void Enter()
    {
        base.Enter();
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PostLocationSettled += OnPostLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.LocationArrived += On_LocationArrived;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
        ModInterface.Events.DateLocationSelected += On_DateLocationSelected;
    }

    public override void Exit()
    {
        base.Exit();
        _pendingState = null;
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PostLocationSettled -= OnPostLocationSettled;
        locationManager.ResolveDollStyles -= OnResolveDollStyles;
        locationManager.LocationArrived -= On_LocationArrived;

        ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;
        ModInterface.Events.DateLocationSelected -= On_DateLocationSelected;
    }

    public override string GetProfileString(PlayerFile playerFile)
    {
        var girlName = playerFile.girlPairDefinition.girlDefinitionOne.girlName;
        var girlName2 = playerFile.girlPairDefinition.girlDefinitionTwo.girlName;

        return playerFile.sidesFlipped 
            ? girlName + " & " + girlName2
            : girlName2 + " & " + girlName;
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
            _pendingState = GameStateId.Hub;
        }
        else
        {
            _pendingState = GameStateId.Sim;
        }
    }

    private void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        _pendingState = GameStateId.Puzzle;
    }

    private Action _continueTransitionCallback;
    private UiDoll _targetDoll;

    public override void DepartTransition(RelativeId? _nextStateId, Action continueTransitionCallback)
    {
        if (_nextStateId.HasValue)
        {
            _pendingState = _nextStateId;
        }

        var dialogTriggerDefinition = _pendingState == GameStateId.Puzzle
            ? Game.Session.Location.dtAskDate
            : Game.Session.Location.dtValediction;

        if (dialogTriggerDefinition != null)
        {
            var args = new RandomDollSelectedArgs();
            ModInterface.Events.NotifyRandomDollSelected(args);
            _targetDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

            _continueTransitionCallback = continueTransitionCallback;
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
        var locationManager = Game.Session.Location;

        if (args.girlPairDef != null)
        {
            Game.Session.Puzzle.puzzleStatus.Reset(locationManager.currentGirlLeft, locationManager.currentGirlRight);
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
}