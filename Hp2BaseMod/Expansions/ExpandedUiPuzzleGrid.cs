using System;
using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Companion class for <see cref="UiPuzzleGrid"/> that replaces the reward calculation
/// pipeline with a scripted-ailment-aware version and exposes flags for ailments to
/// control grid behaviour that was previously hardcoded.
/// </summary>
[Expansion(typeof(UiPuzzleGrid))]
public partial class ExpandedUiPuzzleGrid
{
    [HarmonyPatch(typeof(UiPuzzleGrid))]
    private static class UiPuzzleGridPatch
    {
        [HarmonyPatch("AttemptGirlFocusSwitch")]
        [HarmonyPrefix]
        private static bool AttemptGirlFocusSwitch(UiPuzzleGrid __instance, ref bool __result)
            => ExpandedUiPuzzleGrid.Get(__instance).AttemptGirlFocusSwitch_Prefix(ref __result);

        [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
        [HarmonyPostfix]
        private static void StartPuzzle(UiPuzzleGrid __instance)
            => ExpandedUiPuzzleGrid.Get(__instance).StartPuzzle();

        [HarmonyPatch(nameof(UiPuzzleGrid.EndPuzzle))]
        [HarmonyPostfix]
        private static void EndPuzzle(UiPuzzleGrid __instance)
            => ExpandedUiPuzzleGrid.Get(__instance).EndPuzzle();

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool Update(UiPuzzleGrid __instance)
            => ExpandedUiPuzzleGrid.Get(__instance).Update_Prefix();

        [HarmonyPatch("OnSlotEnter")]
        [HarmonyPostfix]
        private static void OnSlotEnter(UiPuzzleGrid __instance, UiPuzzleSlot slot)
            => ExpandedUiPuzzleGrid.Get(__instance).OnSlotEnter();

        [HarmonyPatch("CheckRoundOver")]
        [HarmonyPrefix]
        private static bool CheckRoundOver(UiPuzzleGrid __instance)
            => ExpandedUiPuzzleGrid.Get(__instance).CheckRoundOver_Prefix();

        [HarmonyPatch("RefreshGirlDolls")]
        [HarmonyPostfix]
        private static void RefreshGirlDolls(UiPuzzleGrid __instance)
            => ExpandedUiPuzzleGrid.Get(__instance).RefreshGirlDolls();

        [HarmonyPatch("ConsumePuzzleSet")]
        [HarmonyPrefix]
        private static bool ConsumePuzzleSet(UiPuzzleGrid __instance, PuzzleSet puzzleSet, bool andDestroy, ref bool __result)
            => ExpandedUiPuzzleGrid.Get(__instance).ConsumePuzzleSet_Prefix(puzzleSet, andDestroy, ref __result);
    }

    public static ExpandedUiPuzzleGrid Get() => Get(Game.Session.Puzzle.puzzleGrid);

    // Focus switch suppression counter, multiple sources can suppress independently.
    private int _suppressFocusSwitchCount;

    /// <summary>
    /// Increments the focus switch suppression counter.
    /// Focus switching is blocked as long as the count is above zero or any
    /// modifier vetoes via <see cref="IUiPuzzleGridModifier.OnAttemptFocusSwitch"/>.
    /// </summary>
    public void SuppressFocusSwitch() => _suppressFocusSwitchCount++;

    /// <summary>
    /// Decrements the focus switch suppression counter.
    /// Has no effect if the counter is already zero.
    /// </summary>
    public void UnsuppressFocusSwitch()
    {
        if (_suppressFocusSwitchCount > 0) _suppressFocusSwitchCount--;
    }

    /// <summary>
    /// When true:
    /// - Stamina sufficiency check is skipped (any move is valid regardless of stamina)
    /// - Stamina is not deducted from the focused girl after a move
    /// - Unfocused girl does not recover stamina after a move
    /// - If the focused girl becomes exhausted or upset due to a resource change,
    ///   her stamina, exhaustion, and upset state are silently reverted
    /// </summary>
    public bool SuppressStaminaCost { get; set; }

    /// <summary>
    /// When true, suppresses the "not enough stamina" warning tooltip.
    /// </summary>
    public bool SuppressStaminaWarning { get; set; }

    /// <summary>
    /// When true, suppresses the "this move will exhaust" warning tooltip.
    /// </summary>
    public bool SuppressExhaustionWarning { get; set; }

    /// <summary>
    /// When true, suppresses the "this move will make her upset" warning tooltip.
    /// </summary>
    public bool SuppressUpsetWarning { get; set; }

    /// <summary>
    /// When true, after becoming upset or exhausted girls will silently revert.
    /// </summary>
    public bool AutoRevertExhaustion { get; set; }

    /// <summary>
    /// Fired at the end of every move resolution, regardless of validity or outcome.
    /// Mirrors UiPuzzleGrid.MoveCompleteEvent but accessible to scripted ailments
    /// and other library consumers without patching.
    /// </summary>
    public event Action MoveCompleteEvent;

    internal void StartPuzzle()
    {
        _core.MoveCompleteEvent += RaiseMoveCompleteEvent;
    }

    private void RaiseMoveCompleteEvent() => MoveCompleteEvent?.Invoke();

    internal void EndPuzzle()
    {
        _core.MoveCompleteEvent -= RaiseMoveCompleteEvent;
    }

    private bool AttemptGirlFocusSwitch_Prefix(ref bool __result)
    {
        __result = Game.Session.Ailment.GetExpansion().OnPreFocusSwitch();
        return __result;
    }

    private bool CheckRoundOver_Prefix()
    {
        var args = new AilmentTriggerArgs.CheckRoundOver(this, null);
        var status = _status;

        if (!Game.Session.Puzzle.dateForfeited && status.affection == status.affectionGoal)
		{
			args.RoundState = PuzzleRoundState.SUCCESS;
			args.RoundOver = true;
		}
		else if (Game.Session.Puzzle.dateForfeited 
            || (
                !status.bonusRound && (status.movesRemaining == 0 
                || (status.girlStatusLeft.exhausted && status.girlStatusRight.exhausted))
                )
            )
		{
			if (!Game.Session.Puzzle.puzzleStatus.IsTutorial(false))
			{
				args.RoundState = PuzzleRoundState.FAILURE;
				args.RoundOver = true;
			}
            args.ReviveGirls = true;
            args.CheckChanges = true;
		}

        var ailmentManager = Game.Session.Ailment.GetExpansion();
        ailmentManager.OnCheckRoundOver(args);

        _roundState = args.RoundState;
        _roundOver = args.RoundOver;
        if (args.ReviveGirls) status.ReviveGirls();
        if (args.CheckChanges) status.CheckChanges();

        return false;
    }

    private void RefreshGirlDolls()
    {
        if (AutoRevertExhaustion)
        {
            _dollLeft.SetExhaustion(false, false, true);
            _dollRight.SetExhaustion(false, false, true);
            _status.ReviveGirls();
        }
    }

    /// <summary>
    /// When SuppressStaminaCost is active and the game is in MOVING state on mouse-up,
    /// reproduces the original move processing loop without stamina deduction,
    /// stamina sufficiency check, or unfocused stamina recovery.
    /// Returns false to skip the original Update only when we handle the move.
    /// </summary>
    private bool Update_Prefix()
    {
        if (!SuppressStaminaCost) return true;

        if (!Game.Session.Location.AtLocationType(LocationType.DATE)
            || !Game.Session.Puzzle.isPuzzleActive
            || Game.Manager.Time.IsPaused(_core.pauseDefinition)
            || !Input.GetMouseButtonUp(0))
        {
            return true;
        }

        var state = (PuzzleGameState)f_state.GetValue(_core);
        if (state != PuzzleGameState.MOVING) return true;

        WarningTooltip(null);

        _core.guideContainer.HideMovementGuides();

        var status = f_status.GetValue(_core) as PuzzleStatus;
        var moveMatchSet = f_moveMatchSet.GetValue(_core) as PuzzleSet;
        var moveSlotFrom = f_moveSlotFrom.GetValue(_core) as UiPuzzleSlot;
        var moveSlotTo = f_moveSlotTo.GetValue(_core) as UiPuzzleSlot;

        var girlStatusFocused = status.girlStatusFocused;
        var girlStatusUnfocused = status.girlStatusUnfocused;

        // Stamina sufficiency check is skipped, any move is valid.
        bool validMove = moveMatchSet != null
            && Game.Session.Puzzle.TutorialStepCheck(moveSlotFrom, moveSlotTo);

        if (validMove)
        {
            int movesCost = moveMatchSet.GetMovesCost();
            int staminaCostRawFull = moveMatchSet.GetStaminaCost(true, true);

            Game.Session.gameCanvas.effectsContainerLow.dragCursor.Deactivate(null);
            moveMatchSet.DimTokens();
            moveSlotFrom.token.Show();

            _moveSlotFrom = null;

            foreach(var slot in _moveSlots)
            {
                slot.ApplyTempToken();
            }

            var moveModifier = Game.Session.Ailment.Trigger(moveMatchSet);
            _moveHasBeenMade = true;

            bool isBigMatch = _core.ConsumePuzzleSet(moveMatchSet, true);

            if (!status.bonusRound)
            {
                // Apply move cost, still respected even without stamina cost.
                if (movesCost > 0 && !moveModifier.blockMoveCost)
                {
                    status.AddResourceValue(PuzzleResourceType.MOVES, -movesCost, girlStatusFocused.altGirl);
                }

                // Stamina deduction and unfocused stamina recovery are skipped entirely.
            }

            Game.Session.Ailment.Trigger(AilmentTriggerType.POST_MOVE, null);

            if (moveModifier.postSwitchGirlFocus
                && girlStatusFocused == status.girlStatusFocused
                && !girlStatusUnfocused.exhausted)
            {
                status.SetGirlFocus(girlStatusUnfocused.altGirl);
            }

            status.CheckChanges();

            if (girlStatusFocused == status.girlStatusFocused
                && staminaCostRawFull > 1
                && isBigMatch
                && moveMatchSet.HasMatchWithResourceType(PuzzleResourceType.BROKEN, true))
            {
                Game.Session.gameCanvas.GetDoll(girlStatusFocused.altGirl)
                    .ReadDialogTrigger(Game.Session.Puzzle.dtBigMove, DialogLineFormat.UNCHECKED, -1);
                Game.Manager.Audio.Play(
                    AudioCategory.SOUND,
                    _core.sfxsTokenBigMatch[UnityEngine.Random.Range(0, _core.sfxsTokenBigMatch.Length)],
                    _core.pauseDefinition);
            }

            _moveMatchSet = null;
            ClearMoveSlots();
        }
        else
        {
            // Invalid move, mirror original reset path exactly.
            if (moveMatchSet != null)
            {
                moveMatchSet.DimTokens();
            }

            if (moveSlotTo != null && moveSlotTo != moveSlotFrom)
            {
                Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, _core.pauseDefinition);
            }
            else
            {
                Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.DragDrop.sfxDragCancel, null);
            }

            _moveMatchSet = null;

            ClearMoveSlots();

            Game.Session.gameCanvas.effectsContainerLow.dragCursor.Deactivate(moveSlotFrom.token.GetTokenSprite());

            var resetTweener = Game.Session.gameCanvas.effectsContainerLow.dragCursor.rectTransform
                .DOMove(moveSlotFrom.rectTransform.position, 0.25f, false)
                .SetEase(Ease.OutSine)
                .OnComplete(OnResetAnimationComplete);

            _resetTweener = resetTweener;
            Game.Manager.Time.Play(resetTweener, _core.pauseDefinition, 0f);
            _isResetting = true;
            _core.ChangeState(PuzzleGameState.RESETTING);
        }

        MoveCompleteEvent?.Invoke();
        return false;
    }

    private void OnSlotEnter()
    {
        if (!SuppressStaminaWarning && !SuppressExhaustionWarning && !SuppressUpsetWarning)
        {
            return;
        }

        if (_state != PuzzleGameState.MOVING) return;

        var moveMatchSet = _moveMatchSet;
        if (moveMatchSet == null) return;

        var status = _status;

        if (SuppressStaminaWarning
            && status.girlStatusFocused.stamina < moveMatchSet.GetStaminaCost(false, false))
        {
            _warningCheck = false;
        }
        else if (SuppressUpsetWarning
            && (moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.noSpawnMatchTokenDef)
                || moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.extraNoSpawnMatchTokenDefs))
            && !status.girlStatusFocused.HasAilment(Game.Session.Puzzle.brokenProtectionAilmentDefinition, true))
        {
            _warningCheck = false;
        }
        else if (SuppressExhaustionWarning
            && status.girlStatusFocused.stamina == moveMatchSet.GetStaminaCost(false, false))
        {
            _warningCheck = false;
        }
    }

    private bool ConsumePuzzleSet_Prefix(PuzzleSet puzzleSet, bool andDestroy, ref bool __result)
	{
		Game.Session.Ailment.Trigger(AilmentTriggerType.PRE_CONSUME, null);
		_matchComboCount++;//reset to 0 when changing to inactive state

		var slotRewards = puzzleSet.GetExpansion().GetRewards();
		var hasPositiveReward = false;
		var hasNegativeReward = false;
        var statusExp = _status.GetExpansion();
		foreach (var slot_reward in slotRewards)
		{
            foreach (var reward in slot_reward.Value.Rewards)
            {
                reward.Apply(this, statusExp);
                var energyTrailBehavior = UnityEngine.Object.Instantiate(_core.energyTrailPrefab);
                _energyTrails.Add(energyTrailBehavior);
                energyTrailBehavior.CompleteEvent += OnEnergyTrailComplete;
                energyTrailBehavior.Init(
                    reward.ZeroedValue
                        ? EnergyTrailFormat.START 
                        : EnergyTrailFormat.FULL, 
                    reward, slot_reward.Key);

                if (reward.Negative)
                {
                    hasNegativeReward = true;
                }
                else
                {
                    hasPositiveReward = true;
                } 
            }
            
		}

		var onlyPositiveRewards = hasPositiveReward && !hasNegativeReward;
		Game.Manager.Audio.Play(AudioCategory.SOUND, _core.sfxsTokenMatch[Mathf.Min(_matchComboCount, _core.sfxsTokenMatch.Length - 1)], _core.pauseDefinition);
		List<TokenDefinition> list = new List<TokenDefinition>();

        //gross
		if (_status.girlStatusFocused.HasAilment(Game.Session.Puzzle.brokenProtectionAilmentDefinition, true))
		{
			list.Add(Game.Session.Puzzle.noSpawnMatchTokenDefinition);
		}
		List<TokenDefinition> tokenDefsInSet = puzzleSet.GetTokenDefsInSet(list);

		for (int i = 0; i < tokenDefsInSet.Count; i++)
		{
			Game.Manager.Audio.Play(AudioCategory.SOUND, tokenDefsInSet[i].sfxMatch, _core.pauseDefinition);
		}
		Game.Session.Ailment.Trigger(AilmentTriggerType.POST_CONSUME, null);

		if (!_status.bonusRound && onlyPositiveRewards && _matchComboCount % 3 == 0)
		{
			UiDoll doll = Game.Session.gameCanvas.GetDoll(_status.altGirlFocused);
			if (!doll.IsTalking(false))
			{
				doll.ReadDialogTrigger(Game.Session.Puzzle.dtBigMove, DialogLineFormat.UNCHECKED, -1);
			}
		}
		_status.CheckChanges();

		if (andDestroy)
		{
            foreach (var slot in puzzleSet.allSlots)
            {
                TokenDefinition replaceDef;
                bool replaceUpgraded;

                if (slotRewards.TryGetValue(slot, out var slotReward))
                {
                    replaceDef = slotReward.ReplaceDefinition;
                    replaceUpgraded = slotReward.ReplaceUpgraded;
                }
                else
                {
                    replaceDef = null;
                    replaceUpgraded = false;
                }

				DestroyToken(slot,replaceDef, replaceUpgraded);
            }
		}

		if (_status.bonusRound && !Game.Session.Puzzle.puzzleStatus.IsTutorial(false))
		{
			_core.AttemptGirlFocusSwitch();
		}

		Game.Session.Cutscenes.standbyProceed = true;
		__result = onlyPositiveRewards;

        return false;
	}
}
