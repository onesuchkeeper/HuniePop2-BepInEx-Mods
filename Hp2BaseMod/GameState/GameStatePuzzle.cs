using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// Active while the player is on a date.
/// What every puzzle shares lives here; what differs between kinds of puzzle (normal, boss, ...) lives in the <see cref="IPuzzleState"/>
/// chosen at each arrival.
/// </summary>
public sealed class GameStatePuzzle : GameStateLocation, IPuzzleState
{
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new GameStatePuzzle(payload.InitialPuzzleStateId);

        public string GetProfileString(PlayerFile playerFile)
        {
            var pair = playerFile.girlPairDefinition;

            return pair == null
                ? "On a date"
                : $"On a date with {pair.GetExpansion().GetNames(playerFile.sidesFlipped)}";
        }

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            error = "Cannot load directly into puzzle state";
            return false;
        }
    }

    [Serializable]
    public class Payload(RelativeId initialPuzzleStateId) : IGameStatePayload
    {
        public RelativeId InitialPuzzleStateId = initialPuzzleStateId;
    }

    private RelativeId _puzzleStateId;
    private IFullPuzzleState _puzzleState;

    public override RelativeId Id => GameStateId.Puzzle;

    public override bool IsSavable => false;

    public override bool AllowGifts => _puzzleState.AllowsGift;

    public bool UnlocksStylesAfterRewards => _puzzleState.UnlocksStylesAfterRewards;

    public bool CreatesReplacementTokens => _puzzleState.CreatesReplacementTokens;

    public bool FailsWhenOutOfMoves => _puzzleState.FailsWhenOutOfMoves;

    public bool AllowsGift => _puzzleState.AllowsGift;

    public bool SwitchesFocusInBonusRound => _puzzleState.SwitchesFocusInBonusRound;

    // ---- what the player can use here ----
    //
    // Every kind of puzzle answers these the same way, except where the puzzle state has a say: the focus buttons (see
    // DollFocusIsInteractive). A kind of puzzle that needs more can be given its own members on IPuzzleState and forwarded here.

    public override bool HidesCellphone(UiCellphone cellphone) => false;

    // the slots are ready once the date is over
    public override FinderStatus GetFinderStatus()
        => FinderStatus.Present(Game.Session.Puzzle.puzzleStatus.gameOver, 1);

    public override bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot) => false;

    // the store can be browsed, and says so
    public override bool AllowsStore => true;
    public override bool StoreShowsNotice => true;

    public override bool AllowsPairApp => true;
    public override bool HasGirlsPresent => true;

    public override string GetGiftError(ItemDefinition item) => DateGiftError;

    public override bool DialogIgnoresCellphone => false;

    // A puzzle whose focus is locked has no focus button to press: the player would be offered a button that does nothing.
    public override bool DollFocusIsInteractive => _puzzleState.LockedAltGirlFocus == null;

    public override bool HintsSimStaminaCost => false;

    // the puzzle manager has its own say (PuzzleManager.CanPause), this state adds nothing
    public override bool AllowsPause => true;

    private bool _disposed;

    public GameStatePuzzle(RelativeId initialPuzzleState)
        :base()
    {
        SetPuzzleState(initialPuzzleState);

        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationSettled += OnPreLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.ArrivalCompleted += On_ArrivalCompleted;
        locationManager.LocationArrived += On_LocationArrived;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            _pendingState = null;
            var locationManager = Game.Session.Location.GetExpansion();
            locationManager.PreLocationSettled -= OnPreLocationSettled;
            locationManager.ResolveDollStyles -= OnResolveDollStyles;
            locationManager.ArrivalCompleted -= On_ArrivalCompleted;
            locationManager.LocationArrived -= On_LocationArrived;

            ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;

            _puzzleState.Dispose();
            _puzzleState = null;
        }

        base.Dispose(disposing);
    }

    private void OnPreLocationSettled(LocationSettledArgs args)
        => _puzzleState.OnLocationSettled(args);

    private void OnResolveDollStyles(ResolveDollStylesArgs args)
        => _puzzleState.TryResolveDollStyles(args);

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        // I'd like a better way to check this. Maybe I could add
        // a flag to the hub slot itself?
        if (slot.locationDefinition?.ModId() == LocationId.HotelRoom)
        {
            _pendingState = new GameStateHub.Payload();
        }
        else
        {
            _pendingState = new GameStateSim.Payload(slot.playerFileFinderSlot.girlPairDefinition.ModId());
        }
    }

    public override void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        if (payload != null)
        {
            _pendingState = payload;
        }
        
        continueTransitionCallback();
    }

    private void On_ArrivalCompleted(ArrivalCompletedArgs args)
    {
        Game.Session.Puzzle.puzzleGrid.ailmentsContainer.Activate();
        _puzzleState.OnArrivalCompleted(args);
    }

    private void On_LocationArrived(LocationArriveArgs args)
    {
        // A puzzle is ended by EndPuzzle before the player leaves a date. One that is still active here was started in a state or at a
        // location that never ended it, and PuzzleManager.StartPuzzle will skip starting this one (see ExpandedPuzzleManager).
        if (Game.Session.Puzzle.isPuzzleActive)
        {
            ModInterface.Log.Warning("Arriving at a puzzle while the previous puzzle is still active. StartPuzzle will be skipped for this arrival.");
        }

        _puzzleState.OnLocationArrived(args);

        FinishArrival(args);
    }

    public void SetPuzzleState(RelativeId puzzleStateId)
    {
        _puzzleState?.Dispose();
        _puzzleState = null;

        if (!ModInterface.GameData.PuzzleStates.TryGetValue(puzzleStateId, out var puzzleStateFactory))
        {
            throw new ArgumentException(nameof(puzzleStateId));
        }

        _puzzleState = puzzleStateFactory.Create(this);
        _puzzleStateId = puzzleStateId;
    }

    public override IGameStatePayload GeneratePayload() 
        => new Payload(_puzzleStateId);

    public int GetPostRewardFruitCount(int baseFruitCount, out bool puzzleFailed) 
        => _puzzleState.GetPostRewardFruitCount(baseFruitCount, out puzzleFailed);

    public void GetPostRewardFruits(int k, out ItemDefinition itemDefinition, out ItemDefinition itemDefinition2) 
        => _puzzleState.GetPostRewardFruits(k, out itemDefinition, out itemDefinition2);

    public bool AllowsMove(UiPuzzleSlot from, UiPuzzleSlot to) => _puzzleState.AllowsMove(from, to);

    // ---- everything below is the puzzle state's, asked for by name ----

    public bool UnlocksStylesAfterRoundClear => _puzzleState.UnlocksStylesAfterRoundClear;

    public bool AllowsForfeit => _puzzleState.AllowsForfeit;

    public bool IsRepeatDate => _puzzleState.IsRepeatDate;

    public bool AllowsAction(PuzzleTutorialStepType action) => _puzzleState.AllowsAction(action);

    public bool AllowsTokenPickup(UiPuzzleSlot slot) => _puzzleState.AllowsTokenPickup(slot);

    public bool SuppressesDollDialog => _puzzleState.SuppressesDollDialog;

    public bool TryGetHeaderLabel(out PuzzleHeaderLabel label) => _puzzleState.TryGetHeaderLabel(out label);

    public void OnPhotoWindowClosed() => _puzzleState.OnPhotoWindowClosed();

    public void OnPuzzleStarting() => _puzzleState.OnPuzzleStarting();

    public void OnRoundOver(PuzzleRoundOverContext context) => _puzzleState.OnRoundOver(context);

    public void OnNextRound() => _puzzleState.OnNextRound();

    public void OnPuzzleStateChanged() => _puzzleState.OnPuzzleStateChanged();

    /// <summary>
    /// Called once the base game's bookkeeping for ending the puzzle is done (see ExpandedPuzzleManager). Where the player goes next depends on the kind of puzzle.
    /// </summary>
    public void OnPuzzleEnded(bool succeeded) => _puzzleState.OnPuzzleEnded(succeeded);

    // ---- rules of the status ----

    public int GetInitialMoves(SettingDifficulty difficulty) => _puzzleState.GetInitialMoves(difficulty);

    public int GetMaxMoves(SettingDifficulty difficulty) => _puzzleState.GetMaxMoves(difficulty);

    public int MinMovesRemaining => _puzzleState.MinMovesRemaining;

    public bool ChecksStaminaFreezeOnStart => _puzzleState.ChecksStaminaFreezeOnStart;

    public bool PopulatesRandomAilments => _puzzleState.PopulatesRandomAilments;

    public bool IsBonusRound(int roundIndex, int girlsRemaining) => _puzzleState.IsBonusRound(roundIndex, girlsRemaining);

    public void TakeRoundGirls(List<GirlDefinition> girlList, GirlDefinition[] roundGirls) => _puzzleState.TakeRoundGirls(girlList, roundGirls);

    public void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay)
        => _puzzleState.GetRoundGoal(roundIndex, bonusRound, out affectionGoal, out bonusDrainDelay);

    public PuzzleStatusType ConditionDateType => _puzzleState.ConditionDateType;

    public bool MatchesTutorialCondition(bool requireScripted) => _puzzleState.MatchesTutorialCondition(requireScripted);

    public bool? LockedAltGirlFocus => _puzzleState.LockedAltGirlFocus;
    public override bool AutosavesOnArrival => true;
    public override bool ShowsHubMenu => false;
    public override bool UseStylesOnDatesFlag => true;
}