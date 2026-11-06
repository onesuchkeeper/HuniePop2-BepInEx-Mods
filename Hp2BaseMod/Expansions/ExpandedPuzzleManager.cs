using System;
using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Replaces every PuzzleManager method that held a check on the kind of puzzle (<c>statusType</c> / <c>IsTutorial</c>). What is generic stays
/// here; what is unique to a kind of puzzle is decided by the current <see cref="IPuzzleState"/>.
/// The puzzle lifecycle (start, round over, next round, end) needs a puzzle state and throws without one. The checks the base game also
/// runs outside of puzzles (the cellphone closing, grid state changes, tutorial step checks) just answer "not applicable" without one.
/// </summary>
[Expansion(typeof(PuzzleManager))]
public partial class ExpandedPuzzleManager
{
    [HarmonyPatch(typeof(PuzzleManager))]
    private static class Patch
    {
        [HarmonyPatch(nameof(PuzzleManager.StartPuzzle))]
        [HarmonyPrefix]
        private static bool StartPuzzle(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).StartPuzzle_Prefix();
            return false;
        }

        [HarmonyPatch("OnRoundOver")]
        [HarmonyPrefix]
        private static bool OnRoundOver(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).OnRoundOver_Prefix();
            return false;
        }

        [HarmonyPatch("OnRoundOverCutsceneComplete")]
        [HarmonyPrefix]
        private static bool OnRoundOverCutsceneComplete(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).OnRoundOverCutsceneComplete_Prefix();
            return false;
        }

        [HarmonyPatch("EndPuzzle")]
        [HarmonyPrefix]
        private static bool EndPuzzle(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).EndPuzzle_Prefix();
            return false;
        }

        [HarmonyPatch("OnNewRoundCutsceneComplete")]
        [HarmonyPrefix]
        private static bool OnNewRoundCutsceneComplete(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).OnNewRoundCutsceneComplete_Prefix();
            return false;
        }

        [HarmonyPatch(nameof(PuzzleManager.ForfeitDate))]
        [HarmonyPrefix]
        private static bool ForfeitDate(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).ForfeitDate_Prefix();
            return false;
        }

        [HarmonyPatch("OnCellphoneClosed")]
        [HarmonyPrefix]
        private static bool OnCellphoneClosed(PuzzleManager __instance)
        {
            ExpandedPuzzleManager.Get(__instance).OnCellphoneClosed_Prefix();
            return false;
        }

        [HarmonyPatch("OnPuzzleStateChanged")]
        [HarmonyPrefix]
        private static bool OnPuzzleStateChanged()
        {
            (ModInterface.GameState.CurrentState as IPuzzleState)?.OnPuzzleStateChanged();
            return false;
        }

        // The tutorial's script is the tutorial state's. Everything else may do anything.
        [HarmonyPatch(nameof(PuzzleManager.TutorialStepCheck), [typeof(UiPuzzleSlot), typeof(UiPuzzleSlot)])]
        [HarmonyPrefix]
        private static bool TutorialStepCheck(UiPuzzleSlot fromSlot, UiPuzzleSlot toSlot, ref bool __result)
        {
            __result = (ModInterface.GameState.CurrentState as IPuzzleState)?.AllowsMove(fromSlot, toSlot) ?? true;
            return false;
        }

        [HarmonyPatch(nameof(PuzzleManager.TutorialStepCheck), [typeof(PuzzleTutorialStepType)])]
        [HarmonyPrefix]
        private static bool TutorialStepCheck(PuzzleTutorialStepType stepType, ref bool __result)
        {
            __result = (ModInterface.GameState.CurrentState as IPuzzleState)?.AllowsAction(stepType) ?? true;
            return false;
        }

        // Preloading the tutorial's grid is the tutorial state's (see PuzzleStateTutorial.OnPuzzleStarting).
        [HarmonyPatch("PreloadTutorialTokens")]
        [HarmonyPrefix]
        private static bool PreloadTutorialTokens() => false;

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(PuzzleManager __instance)
            => ExpandedPuzzleManager.Destroy(__instance);
    }

    /// <summary>
    /// True from the moment a round is over until the next round starts (or the puzzle ends). The result of a round is fixed for that
    /// whole time: a match that settles while the round over and new round cutscenes play must not decide the round again, which
    /// would start the round over cutscenes a second time and leave their subscriptions dangling.
    /// </summary>
    internal bool IsRoundOverPending { get; private set; }

    // The cutscene events are subscribed through these so that a handler is never subscribed twice, and a stale one can always be dropped.
    private void SubscribeRoundOverCutsceneComplete()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnRoundOverCutsceneComplete;
        Game.Session.Cutscenes.CutsceneCompleteEvent += OnRoundOverCutsceneComplete;
    }

    private void SubscribeNewRoundCutsceneComplete()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnNewRoundCutsceneComplete;
        Game.Session.Cutscenes.CutsceneCompleteEvent += OnNewRoundCutsceneComplete;
    }

    private void DropRoundCutsceneSubscriptions()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnRoundOverCutsceneComplete;
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnNewRoundCutsceneComplete;
        IsRoundOverPending = false;
    }

    private static IPuzzleState RequirePuzzleState(string caller)
        => ModInterface.GameState.CurrentState as IPuzzleState
        ?? throw new InvalidOperationException($"{caller}: a puzzle can only run in a puzzle state, but the current state is {ModInterface.GameState.CurrentState?.Id}.");

    private void StartPuzzle_Prefix()
    {
        // GameStatePuzzle logs a warning when it arrives with a puzzle still active
        if (_isPuzzleActive) return;

        var puzzleState = RequirePuzzleState(nameof(StartPuzzle_Prefix));
        var grid = _core.puzzleGrid;
        var status = _core.puzzleStatus;

        DropRoundCutsceneSubscriptions();
        grid.RoundOverEvent -= OnRoundOver;
        grid.RoundOverEvent += OnRoundOver;
        status.PopulateDateGifts();
        status.SetGirlFocusByStamina();
        _isPuzzleActive = true;
        _isRepeatDate = puzzleState.IsRepeatDate;
        _dateForfeited = false;
        grid.preloadedTokens.Clear();

        // anything that has to be in place before the grid creates its first tokens
        puzzleState.OnPuzzleStarting();

        grid.StartPuzzle();
    }

    private void OnRoundOver_Prefix()
    {
        var puzzleState = RequirePuzzleState(nameof(OnRoundOver_Prefix));
        var grid = _core.puzzleGrid;

        if (IsRoundOverPending)
        {
            ModInterface.Log.Warning("A round over was raised while the previous one is still being presented. Ignoring it.");
            return;
        }

        var succeededBefore = grid.roundState == PuzzleRoundState.SUCCESS;
        var context = new PuzzleRoundOverContext(succeededBefore, _core.cutsceneFailure);

        puzzleState.OnRoundOver(context);

        _roundOverCutscene = context.RoundOverCutscene;
        _newRoundCutscene = context.NewRoundCutscene;

        // PuzzleStatus.gameOver only ever turns on; the context may turn it off
        _core.puzzleStatus.GetExpansion()._gameOver = context.GameOver;

        if (context.RoundSucceeded != succeededBefore)
        {
            grid.GetExpansion()._roundState = context.RoundSucceeded
                ? PuzzleRoundState.SUCCESS
                : PuzzleRoundState.FAILURE;
        }

        IsRoundOverPending = true;
        SubscribeRoundOverCutsceneComplete();
        Game.Session.Cutscenes.StartCutscene(_roundOverCutscene, null);
    }

    private void OnRoundOverCutsceneComplete_Prefix()
    {
        var puzzleState = RequirePuzzleState(nameof(OnRoundOverCutsceneComplete_Prefix));

        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnRoundOverCutsceneComplete;

        // a stale subscription: there is no round over being presented
        if (!_isPuzzleActive || !IsRoundOverPending) return;

        var status = _core.puzzleStatus;
        if (status.gameOver)
        {
            EndPuzzle();
            return;
        }

        status.GetExpansion().NextRound(-1, false);
        if (!status.bonusRound)
        {
            status.PopulateAilments();
            _core.puzzleGrid.ailmentsContainer.RePopulate();
            status.PopulateDateGifts();
            status.SetGirlFocusByStamina();
        }

        puzzleState.OnNextRound();

        if (!status.bonusRound)
        {
            Game.Session.Location.ResetDolls(false);
        }

        Game.Session.gameCanvas.header.Refresh(true);
        Game.Session.gameCanvas.cellphone.Refresh(true);
        SubscribeNewRoundCutsceneComplete();
        Game.Session.Cutscenes.StartCutscene(_newRoundCutscene, null);
    }

    private void OnNewRoundCutsceneComplete_Prefix()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnNewRoundCutsceneComplete;

        // a stale subscription must never start a round on a board that is not in a puzzle
        if (!_isPuzzleActive || !IsRoundOverPending) return;

        IsRoundOverPending = false;
        _core.puzzleGrid.StartNewRound();
    }

    private void EndPuzzle_Prefix()
    {
        var puzzleState = RequirePuzzleState(nameof(EndPuzzle_Prefix));
        var grid = _core.puzzleGrid;

        DropRoundCutsceneSubscriptions();
        grid.RoundOverEvent -= OnRoundOver;
        var succeeded = grid.roundState == PuzzleRoundState.SUCCESS;
        grid.EndPuzzle();

        // The dolls leave the puzzle as they entered it, whatever the last refresh left them as (focused, not exhausted).
        var canvas = Game.Session.gameCanvas;
        canvas.dollLeft.SetFocus(true, 0.32f);
        canvas.dollRight.SetFocus(true, 0.32f);
        canvas.dollLeft.SetExhaustion(false, false, true);
        canvas.dollRight.SetExhaustion(false, false, true);

        grid.preloadedTokens.Clear();
        _isPuzzleActive = false;
        _isRepeatDate = false;
        _dateForfeited = false;
        _puzzleOffsets.Clear();

        // Where a finished puzzle leads, and which state that needs, depends on the kind of puzzle.
        puzzleState.OnPuzzleEnded(succeeded);

        Game.Session.Ailment.GetExpansion().OnEndPuzzle();
    }

    private void ForfeitDate_Prefix()
    {
        if (!_isPuzzleActive) return;

        if (_core.puzzleGrid.roundOver
            || !RequirePuzzleState(nameof(ForfeitDate_Prefix)).AllowsForfeit
            || _core.puzzleStatus.gameOver
            || _dateForfeited)
        {
            return;
        }

        _dateForfeited = true;
    }

    private void OnCellphoneClosed_Prefix()
    {
        if (!_isPuzzleActive) return;

        if (_core.puzzleGrid.roundOver
            || !RequirePuzzleState(nameof(OnCellphoneClosed_Prefix)).AllowsForfeit
            || _core.puzzleStatus.gameOver
            || !_dateForfeited
            || Game.Manager.Time.IsPaused(_core.puzzleGrid.pauseDefinition))
        {
            return;
        }

        _core.puzzleGrid.ForceCheckRoundEnd();
    }

    private void OnDestroy()
    {
        _core.puzzleStatus?.GetExpansion().Destroy();
    }
}