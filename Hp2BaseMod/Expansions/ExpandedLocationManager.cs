using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(LocationManager))]
[Deprecates(nameof(LocationManager.AtLocationType), $"Removed, and it throws. There is no location type: ask {nameof(ModInterface)}.{nameof(ModInterface.GameState)}.{nameof(ModInterface.GameState.CurrentState)} what you need to know (see {nameof(IGameState)}).")]
//Depreciates _transitions and _currentTransitionType
public partial class ExpandedLocationManager
{
    [HarmonyPatch(typeof(LocationManager))]
    private static class Patch
    {
        [HarmonyPatch(nameof(LocationManager.Arrive))]
        [HarmonyPrefix]
        private static bool PreArrive(LocationManager __instance,
            LocationDefinition locationDef,
            GirlPairDefinition girlPairDef,
            bool sidesFlipped,
            bool initialArrive)
            => ExpandedLocationManager.Get(__instance).Arrive_Prefix(
                locationDef,
                girlPairDef,
                sidesFlipped,
                initialArrive);

        [HarmonyPatch(nameof(LocationManager.Depart))]
        [HarmonyPrefix]
        private static bool PreDepart(LocationManager __instance, ref LocationDefinition locationDef, ref GirlPairDefinition girlPairDef, ref bool sidesFlipped)
            => ExpandedLocationManager.Get(__instance).Depart_Prefix(ref locationDef, ref girlPairDef, ref sidesFlipped);

        [HarmonyPatch("OnLocationSettled")]
        [HarmonyPrefix]
        private static bool OnLocationSettled(LocationManager __instance)
            => ExpandedLocationManager.Get(__instance).LocationSettled_Prefix();

        // Replaces the base game's ResetDolls, which styles the dolls from the location's type.
        [HarmonyPatch(nameof(LocationManager.ResetDolls))]
        [HarmonyPrefix]
        private static bool ResetDolls(LocationManager __instance, bool unload = false)
            => ExpandedLocationManager.Get(__instance).ResetDolls_Prefix(unload);

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(LocationManager __instance)
            => ExpandedLocationManager.Destroy(__instance);

        [HarmonyPatch(nameof(LocationManager.AtLocationType))]
        [HarmonyPrefix]
        private static bool AtLocationType(LocationManager __instance)
            => ExpandedLocationManager.Get(__instance).AtLocationType_Prefix();

        [HarmonyPatch("OnArrivalComplete")]
        [HarmonyPrefix]
        private static bool OnArrivalComplete(LocationManager __instance)
            => ExpandedLocationManager.Get(__instance).OnArrivalComplete_Prefix();

        
    }

    //TODO, move to expanded ui puzzle grid
    [HarmonyPatch(typeof(UiPuzzleGrid))]
    internal static class UiPuzzleGridCellphonePatch
    {
        [HarmonyPatch(nameof(UiPuzzleGrid.RefreshGirlDolls))]
        [HarmonyPostfix]
        private static void RefreshGirlDolls(UiPuzzleGrid __instance)
            => ExpandedLocationManager.RefreshGirlDolls();
    }

    public event Action<LocationSettledArgs> PreLocationSettled;
    public event Action<LocationSettledArgs> PostLocationSettled;
    
    public event Action<LocationArriveCancelArgs> TryLocationArrive;
    public event Action<LocationArriveArgs> PreLocationArrive;
    public event Action<LocationArriveArgs> LocationArrived;
    public event Action<ArrivalCompletedArgs> ArrivalCompleted;

    public event Action<LocationDepartArgs> PreLocationDepart;
    public event Action<ResolveDollStylesArgs> ResolveDollStyles;

    public event Action<LocationArriveSequenceArgs> LocationArriveSequence;
    internal void NotifyLocationArriveSequence(LocationArriveSequenceArgs sequence) => LocationArriveSequence?.Invoke(sequence);

    public event Action<LocationDepartSequenceArgs> LocationDepartSequence;
    internal void NotifyLocationDepartSequence(LocationDepartSequenceArgs sequence) => LocationDepartSequence?.Invoke(sequence);

    /// <summary>
    /// Sets the location manager's current location without raising any event, changing game state, running a transition or
    /// processing any bundle (settling keeps using <see cref="_arrivedLocation"/>).
    /// For callers that fake a location change visually (see <see cref="ModGameData.ChangeLocationCutsceneStep"/>) and need the
    /// manager to agree with what the player sees.
    /// </summary>
    internal void SetCurrentLocationSilently(LocationDefinition locationDef)
    {
        _currentLocation = locationDef;
    }

    /// <summary>
    /// The travel-flag cleanup that <see cref="OnArrivalComplete_Prefix"/> performs, for callers that run their own transition.
    /// </summary>
    internal void EndTransitionlessTravel()
    {
        _isTraveling = false;
        _bgMusicOverride = null;
    }

    private CutsceneDefinition _baseCutsceneMeeting;
    private UiWindow _actionBubblesWindow;

    /// <summary>
    /// The location the most recent real arrival (<see cref="Arrive_Prefix"/>) was for. Settling processes THIS location's depart
    /// bundles rather than whatever location is current when the settle happens: a visual-only location change during the arrival
    /// cutscene (<see cref="ModGameData.ChangeLocationCutsceneStep"/>) updates the current location, but must not cause the new
    /// location's bundles to run.
    /// </summary>
    private LocationDefinition _arrivedLocation;

    /// <summary>
    /// The default anchored position of the puzzle grid, captured on first arrival
    /// before any CellphoneOnLeft repositioning is applied.
    /// </summary>
    private Vector2? _defaultPuzzleGridPosition;

    private void OnInit()
    {
        _actionBubblesWindow = _core.actionBubblesWindow;
        _baseCutsceneMeeting = _core.cutsceneMeeting;

        // The very first arrival in a session has no preceding Depart() call,
        // so SetTransition() (normally called from Depart_Prefix) never runs
        // for it - leaving _transitions[NORMAL] pointed at whatever vanilla
        // default sits there, which doesn't fire LocationArriveSequence/etc.
        // Seed it here so even the initial arrival goes through our transition.
        SetTransition(new ModLocationTransitionNormal());
    }

    /// <summary>
    /// Nothing may ask the location manager what kind of place it is: there is no location type, only game states. Everything
    /// the base game decided from it is asked of the current <see cref="IGameState"/> instead. This is patched in only so that
    /// a caller that was missed fails loudly, with the stack that made the call.
    /// </summary>
    private bool AtLocationType_Prefix()
    {
        var message = "LocationManager.AtLocationType must not be used, there is no location type. "
            + "Ask ModInterface.GameState.CurrentState (see IGameState) for what you need instead.";

        ModInterface.Log.Error(message + "\n" + new System.Diagnostics.StackTrace());
        throw new NotSupportedException(message);
    }

    private bool Arrive_Prefix(
        LocationDefinition locationDef,
        GirlPairDefinition girlPairDef,
        bool sidesFlipped,
        bool initialArrive)
    {
        //prefix
        var previousLocation = _currentLocation;

        // Fallback if locationDef is null before checking its type
        if (locationDef == null)
        {
            ModInterface.Log.Error("Location def was set to null pre arrival, defaulting to hub");
            locationDef = ModInterface.GameData.GetLocation(Locations.HotelRoom);
            girlPairDef = null;
        }

        var args = new LocationArriveArgs()
        {
            locationDef = locationDef,
            girlPairDef = girlPairDef,
            sidesFlipped = sidesFlipped,
            initialArrive = initialArrive,
            cellphoneOnLeft = false,
            arrivalCutscene = _baseCutsceneMeeting,
            previousLocationDef = previousLocation,
        };

        var cancelArgs = new LocationArriveCancelArgs(args);
        TryLocationArrive?.Invoke(cancelArgs);

        if (cancelArgs.Canceled)
        {
            ModInterface.Log.Message("Arrive canceled");
            return false;
        }

        // A state may have queued a transition (a UI selection, a hub departure
        // hook, etc). Resolve it now, before anything else, so that by the time
        // LocationArrived fires below, the *destination* state is the one
        // subscribed and doing the arrival setup - not the one we're leaving.
        if (ModInterface.GameState.CurrentState is GameStateLocation currentState
            && currentState.TryConsumePendingState(out var payload))
        {
            ModInterface.GameState.ChangeState(payload);
        }

        var stateBeforeEvent = ModInterface.GameState.CurrentState;
        PreLocationArrive?.Invoke(args);

        // Events are raised against a snapshot of their handlers: a state entered by a handler does not receive this PreLocationArrive,
        // and the state it replaced still did. Transitions belong in GameStateLocation.QueueTransition, which is applied above.
        if (!ReferenceEquals(stateBeforeEvent, ModInterface.GameState.CurrentState))
        {
            ModInterface.Log.Warning(
                $"The game state changed from {stateBeforeEvent?.Id} to {ModInterface.GameState.CurrentState?.Id} while PreLocationArrive was being raised. " +
                "The new state's PreLocationArrive handlers did not receive this arrival. Queue the transition with GameStateLocation.QueueTransition instead.");
        }

        _core.cutsceneMeeting = args.arrivalCutscene ?? _baseCutsceneMeeting;
        ModInterface.State.CellphoneOnLeft = args.cellphoneOnLeft;

        var strBuilder = new StringBuilder($"Arriving at {args.locationDef.locationName}");
        if (args.girlPairDef == null) strBuilder.Append(" with no pair.");
        else strBuilder.Append($" with {args.girlPairDef.girlDefinitionOne.girlName} and {args.girlPairDef.girlDefinitionTwo.girlName}.");
        strBuilder.Append($" SidesFlipped: {args.sidesFlipped}, InitialArrive: {args.initialArrive}, CellphoneOnLeft: {ModInterface.State.CellphoneOnLeft}");
        ModInterface.Log.Message(strBuilder.ToString());

        //base
        Game.Persistence.playerFile.locationDefinition = args.locationDef;
        Game.Persistence.playerFile.girlPairDefinition = args.girlPairDef;
        Game.Persistence.playerFile.sidesFlipped = args.sidesFlipped;
        _currentLocation = args.locationDef;
        _arrivedLocation = args.locationDef;
        _currentGirlPair = args.girlPairDef;
        _currentSidesFlipped = args.sidesFlipped;
        _transitions[_currentTransitionType].Prep();
        bool gameSaved = false;
        if (!args.initialArrive || Game.Persistence.debugMode)
        {
            Game.Persistence.playerFile.ClearPerishableInventoryItems();
            int daytimeElapsed = Game.Persistence.playerFile.daytimeElapsed;
            int num = Mathf.FloorToInt((float)daytimeElapsed / 4f);
            if (num > 0)
            {
                if (daytimeElapsed != Game.Persistence.playerFile.finderRestockTime)
                {
                    Game.Persistence.playerFile.finderRestockTime = daytimeElapsed;
                    Game.Persistence.playerFile.PopulateFinderSlots();
                }

                if (num != Game.Persistence.playerFile.storeRestockDay)
                {
                    Game.Persistence.playerFile.storeRestockDay = num;
                    Game.Persistence.playerFile.PopulateStoreProducts();
                }
            }

            if (!args.initialArrive && ModInterface.GameState.CurrentState.AutosavesOnArrival)
            {
                Game.Persistence.Apply(loadedFile: true);
                Game.Persistence.SaveGame();
                gameSaved = true;
            }
        }

        // Destination state does its arrival setup here: puzzle status, dolls,
        // arrive-bundle processing, and any cutscene override.
        LocationArrived?.Invoke(args);

        // Only an explicit override (e.g. Sim's first-meeting cutscene) should win.
        // FinishArrival always leaves args.arrivalCutscene null when a state doesn't
        // set one, and that must NOT erase a cutscene queued before this arrival
        // started (e.g. via LogicManager's START_CUTSCENE-while-locked path) - hence
        // the null check, rather than an unconditional write-back.
        if (args.arrivalCutscene != null)
        {
            _arrivalCutscene = args.arrivalCutscene;
        }

        _transitions[_currentTransitionType].Arrive(args.initialArrive, (_currentGirlPair != null || !Game.Session.Puzzle.puzzleStatus.isEmpty) && _arrivalCutscene == null, gameSaved);

        //postfix
        var header = Game.Session.gameCanvas.header;
        var cellphone = Game.Session.gameCanvas.cellphone;
        var puzzleGrid = Game.Session.Puzzle.puzzleGrid;
        var puzzleGridRect = puzzleGrid != null ? puzzleGrid.GetComponent<RectTransform>() : null;

        // Capture the default grid position the first time we see the grid,
        // before any CellphoneOnLeft offset is applied.
        if (puzzleGridRect != null && _defaultPuzzleGridPosition == null)
        {
            _defaultPuzzleGridPosition = puzzleGridRect.anchoredPosition;
        }

        bool onLeft = ModInterface.State.CellphoneOnLeft;

        header.rectTransform.anchoredPosition = new Vector2(
            onLeft ? header.xValues.y : header.xValues.x,
            header.rectTransform.anchoredPosition.y);

        cellphone.rectTransform.anchoredPosition = new Vector2(
            onLeft ? cellphone.xValues.y : cellphone.xValues.x,
            cellphone.rectTransform.anchoredPosition.y);

        if (puzzleGridRect != null && _defaultPuzzleGridPosition.HasValue)
        {
            // Only the "on left" side shifts the grid by the header's x delta; otherwise it sits at its default spot.
            var delta = onLeft ? header.xValues.y - header.xValues.x : 0f;
            puzzleGridRect.anchoredPosition = new Vector2(
                _defaultPuzzleGridPosition.Value.x + delta,
                _defaultPuzzleGridPosition.Value.y);
        }

        return false;
    }

    //Even though there is only 1 transition type in the base game,
    //and the new arg system doesn't rely on _transitions or LocationTransitionType,
    //we will still use the _transitions dict. This is because the base
    //game triggers events and depreciating it would mean we have to
    //replace all those events and subscriptions to them thought
    //so it's simpler just to treat the dict as a single instance.
    //actually nothing subscribes to depart/arrive, I could replace it... TODO
    private void SetTransition(LocationTransition transition)
    {
        var currentTransition = _transitions[LocationTransitionType.NORMAL];

        if (transition == null || currentTransition == transition) return;

        if (currentTransition != null)
        {
            currentTransition.DepartureCompleteEvent -= OnDepartureComplete;
            currentTransition.ArrivalCompleteEvent -= OnArrivalComplete;
        }

        _transitions[LocationTransitionType.NORMAL] = transition;

        transition.DepartureCompleteEvent += OnDepartureComplete;
        transition.ArrivalCompleteEvent += OnArrivalComplete;
    }

    private bool Depart_Prefix(ref LocationDefinition locationDef, ref GirlPairDefinition girlPairDef, ref bool sidesFlipped)
    {
        var args = new LocationDepartArgs()
        {
            from = Game.Persistence.playerFile.locationDefinition,
            to = locationDef,
            girlPairDef = girlPairDef,
            sidesFlipped = sidesFlipped,
            Canceled = false,
            transition = new ModLocationTransitionNormal()
        };

        PreLocationDepart?.Invoke(args);

        if (args.Canceled)
        {
            return false; // Skips base LocationManager.Depart entirely
        }

        SetTransition(args.transition);

        locationDef = args.to;
        girlPairDef = args.girlPairDef;
        sidesFlipped = args.sidesFlipped;
        return true; // Proceed with normal departure
    }

    /// <summary>
    /// Called from <see cref="UiPuzzleGridCellphonePatch.RefreshGirlDolls"/> postfix.
    /// When <see cref="ModInterface.State.CellphoneOnLeft"/> is active the left doll
    /// is hidden. Its focus button is disabled and any hover state is cleared after
    /// every RefreshGirlDolls call, RefreshGirlDolls may re-enable it internally,
    /// and without the explicit OnPointerExit the tooltip can persist.
    /// </summary>
    private static void RefreshGirlDolls()
    {
        if (!ModInterface.State.CellphoneOnLeft) return;
        var focusButton = Game.Session.gameCanvas.dollLeft.focusButton;
        focusButton.Disable();
        focusButton.OnPointerExit(null);
    }

    /// <summary>
    /// Notifies of location settling, allowing the action-bubble window to be
    /// overwritten.
    /// </summary>
    private bool LocationSettled_Prefix()
    {
        ModInterface.Log.Message("ExpandedLocationManager - LocationSettledPrefix");
        var arrivalCutscene = _arrivalCutscene;

        var args = new LocationSettledArgs()
        {
            actionBubblesWindow = _actionBubblesWindow,
            hasArrivalCutscene = arrivalCutscene != null
        };

        PreLocationSettled?.Invoke(args);

        _core.actionBubblesWindow = args.actionBubblesWindow ?? _actionBubblesWindow;

        _isLocked = false;
        Game.Session.Logic.ProcessBundleList((_arrivedLocation ?? _core.currentLocation).departBundleList, false);

        _arrivalCutscene = null;

        PostLocationSettled?.Invoke(args);

        return false;
    }

    /// <summary>
    /// Doll styling is fully delegated to the active IGameState: this just gathers
    /// the current arrival context and fires ResolveDollStyles. See the game states
    /// (GameStateSim, GameStatePuzzle, GameStateHub, GameStateSpecial) for the actual styling algorithms.
    /// </summary>
    private bool ResetDolls_Prefix(bool unload)
    {
        if (unload)
        {
            Game.Session.gameCanvas.dollLeft.UnloadGirl();
            Game.Session.gameCanvas.dollRight.UnloadGirl();
            Game.Session.gameCanvas.dollMiddle.UnloadGirl();
            return false;
        }

        var girlPair = f_currentGirlPair.GetValue(_core) as GirlPairDefinition;
        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;

        // Who is in the dolls. The base game's ResetDolls did this (and styled them from the location's type), and it no longer runs,
        // so it is done here and the states style the dolls. A state that wants a girl nobody here loads (the hub) loads her when
        // ResolveDollStyles is raised.
        if (!puzzleStatus.isEmpty)
        {
            var bossPair = Game.Session.Puzzle.bossGirlPairDefinition;
            var isBoss = girlPair == bossPair;

            Game.Session.gameCanvas.dollLeft.LoadGirl(puzzleStatus.girlStatusLeft.girlDefinition, -1, -1, -1, isBoss ? bossPair.girlDefinitionOne : null);
            Game.Session.gameCanvas.dollRight.LoadGirl(puzzleStatus.girlStatusRight.girlDefinition, -1, -1, -1, isBoss ? bossPair.girlDefinitionTwo : null);
        }
        else if (girlPair != null)
        {
            Game.Session.gameCanvas.dollLeft.LoadGirl(_core.currentGirlLeft, -1, -1, -1, null);
            Game.Session.gameCanvas.dollRight.LoadGirl(_core.currentGirlRight, -1, -1, -1, null);
        }
        else
        {
            Game.Session.gameCanvas.dollLeft.UnloadGirl();
            Game.Session.gameCanvas.dollRight.UnloadGirl();
        }

        Game.Session.gameCanvas.dollMiddle.UnloadGirl();

        var args = new ResolveDollStylesArgs
        {
            Location = f_currentLocation.GetValue(_core) as LocationDefinition,
            GirlPair = girlPair,
            SidesFlipped = f_currentSidesFlipped.GetValue<bool>(_core)
        };

        ResolveDollStyles?.Invoke(args);
        return false;
    }

    private bool OnArrivalComplete_Prefix()
    {
        _isTraveling = false;
        _bgMusicOverride = null;
        Game.Session.gameCanvas.overlayCanvasGroup.blocksRaycasts = false;

        var args = new ArrivalCompletedArgs()
        {
            ArrivalCutscene = _arrivalCutscene
        };

        ArrivalCompleted?.Invoke(args);

        _arrivalCutscene = args.ArrivalCutscene;

        if (_arrivalCutscene != null)
        {
            Game.Session.Cutscenes.CutsceneCompleteEvent += OnCutsceneComplete_Hook;
            Game.Session.Cutscenes.StartCutscene(_arrivalCutscene);
        }
        else
        {
            OnLocationSettled();
        }

        return false;
    }

    // OnCutsceneComplete forwards to the core's method, which only unsubscribes the core's own delegate. Without unsubscribing this
    // hook here, every arrival cutscene leaves one more subscription behind and every later cutscene settles the location once per leak.
    private void OnCutsceneComplete_Hook()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnCutsceneComplete_Hook;
        OnCutsceneComplete();
    }
}