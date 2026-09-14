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
    private static class Patch
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

        [HarmonyPatch("ConsumePuzzleSet")]
        [HarmonyPrefix]
        private static bool ConsumePuzzleSet(UiPuzzleGrid __instance, PuzzleSet puzzleSet, bool andDestroy, ref bool __result)
            => ExpandedUiPuzzleGrid.Get(__instance).ConsumePuzzleSet_Prefix(puzzleSet, andDestroy, ref __result);

        [HarmonyPatch("CreateToken")]
        [HarmonyPrefix]
        public static void CreateToken_Prefix(UiPuzzleGrid __instance, int col, bool noMatch)
            => ExpandedUiPuzzleGrid.Get(__instance).CreateToken_Prefix(col, noMatch);
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
    /// Fired at the end of every move resolution, regardless of validity or outcome.
    /// Mirrors UiPuzzleGrid.MoveCompleteEvent but accessible to scripted ailments
    /// and other library consumers without patching.
    /// </summary>
    public event Action MoveCompleteEvent;

    private void StartPuzzle()
    {
        var statusExp = _status.GetExpansion();
        statusExp.SuppressStateDialog = false;

        _core.MoveCompleteEvent += RaiseMoveCompleteEvent;
    }

    private void RaiseMoveCompleteEvent()
    {
        // Notify both girls' state machines on move completion
        _status.girlStatusLeft?.GetExpansion().OnMoveCompleted();
        _status.girlStatusRight?.GetExpansion().OnMoveCompleted();
        
        MoveCompleteEvent?.Invoke();
    }

    private void EndPuzzle()
    {
        var statusExp = _status.GetExpansion();
        statusExp.SuppressStateDialog = true;

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

        // Set round flags before reviving girls
        // that way the revive knows it's ending and 
        // doesn't play the recovery lines
        _roundState = args.RoundState;
        _roundOver = args.RoundOver;

        if (args.RoundOver || args.ReviveGirls)
        {
            var statusExp = status.GetExpansion();
            statusExp.SuppressStateDialog = true;

            if (args.ReviveGirls) status.ReviveGirls();

            statusExp.SuppressStateDialog = false;
        }
        
        if (args.CheckChanges) status.CheckChanges();

        return false;
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

        var state = _state;
        if (state != PuzzleGameState.MOVING) return true;

        WarningTooltip(null);

        _core.guideContainer.HideMovementGuides();

        var status = _status;
        var moveMatchSet = _moveMatchSet;
        var moveSlotFrom = _moveSlotFrom;
        var moveSlotTo = _moveSlotTo;

        var girlStatusFocused = status.girlStatusFocused;
        var girlStatusUnfocused = status.girlStatusUnfocused;

        // Stamina sufficiency check is skipped, any move is valid.
        bool validMove = moveMatchSet != null
            && Game.Session.Puzzle.TutorialStepCheck(moveSlotFrom, moveSlotTo);

        if (validMove)
        {
            var moveMatchSetExp = moveMatchSet.GetExpansion();
            var statusExp = status.GetExpansion();
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
                    statusExp.AddResourceValue(PuzzleResourceId.Moves, -movesCost, girlStatusFocused.altGirl);
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
                && moveMatchSetExp.HasMatchWithResource(PuzzleResourceId.Broken, true)
                && !girlStatusFocused.GetExpansion().State.SatisfiesCondition(AbilityStepConditionType.IS_UPSET)) // Skip big move line if girl is upset
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

        RaiseMoveCompleteEvent();
        return false;
    }

    private void OnSlotEnter()
    {
        if (_state != PuzzleGameState.MOVING) return;

        var moveMatchSet = _moveMatchSet;
        if (moveMatchSet == null) return;

        var status = _status;
        var focusedGirl = status.girlStatusFocused;
        var staminaCost = moveMatchSet.GetStaminaCost(false, false);

        // Calculate raw baseline warning states
        bool rawInsufficientStamina = focusedGirl.stamina < staminaCost;
        bool rawWillExhaust = focusedGirl.stamina == staminaCost;
        bool rawWillMakeUpset = (moveMatchSet.HasMatchWithTokenDef(focusedGirl.noSpawnMatchTokenDef)
                || moveMatchSet.HasMatchWithTokenDef(focusedGirl.extraNoSpawnMatchTokenDefs))
            && !focusedGirl.HasAilment(Game.Session.Puzzle.brokenProtectionAilmentDefinition, true);

        // Build args and dispatch non-mutating query event
        var args = new AilmentTriggerArgs.MoveWarningArgs(
            moveMatchSet,
            focusedGirl,
            this,
            rawWillMakeUpset,
            rawWillExhaust,
            rawInsufficientStamina);

        Game.Session.Ailment.GetExpansion().NotifyQueryMoveWarning(args);

        // Respect grid-level suppression flags first
        if (SuppressStaminaWarning) args.InsufficientStamina = false;
        if (SuppressExhaustionWarning) args.WillExhaust = false;
        if (SuppressUpsetWarning) args.WillMakeUpset = false;

        // Update UI warning status
        if (args.InsufficientStamina || args.WillMakeUpset || args.WillExhaust)
        {
            _warningCheck = true;

            if (!string.IsNullOrEmpty(args.OverrideWarningText))
            {
                WarningTooltip(args.OverrideWarningText);
            }
        }
        else
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

    // Grid's own collection of excluded tokens (kept separate from PuzzleStatusGirl.invalidTokenDefs)
    public HashSet<TokenDefinition> ExcludedTokenDefs { get; } = new HashSet<TokenDefinition>();

    public void AddExcludedToken(TokenDefinition tokenDef)
    {
        if (tokenDef != null) ExcludedTokenDefs.Add(tokenDef);
    }

    public void RemoveExcludedToken(TokenDefinition tokenDef)
    {
        if (tokenDef != null) ExcludedTokenDefs.Remove(tokenDef);
    }

    public void ClearExcludedTokens() => ExcludedTokenDefs.Clear();

    /// <summary>
    /// Combines girl-specific invalid tokens, grid-level exclusions, and predicate failures 
    /// into a single list without mutating PuzzleStatusGirl.invalidTokenDefs.
    /// </summary>
    public List<TokenDefinition> GetTotalExcludedTokenDefs(PuzzleStatus status)
    {
        var excluded = new List<TokenDefinition>();

        // 1. Read-only merge of the focused girl's invalid tokens
        if (status?.girlStatusFocused?.invalidTokenDefs != null)
        {
            foreach (var tokenDef in status.girlStatusFocused.invalidTokenDefs)
            {
                if (tokenDef != null && !excluded.Contains(tokenDef))
                    excluded.Add(tokenDef);
            }
        }

        // 2. Merge grid-managed exclusions
        foreach (var tokenDef in ExcludedTokenDefs)
        {
            if (tokenDef != null && !excluded.Contains(tokenDef))
                excluded.Add(tokenDef);
        }

        // 3. Evaluate CanSpawnPredicate on all active status tokens
        if (status?.tokenStatus != null)
        {
            foreach (var statusToken in status.tokenStatus)
            {
                var tokenDef = statusToken.tokenDefinition;
                if (tokenDef == null || excluded.Contains(tokenDef)) continue;

                var tokenExp = tokenDef.GetExpansion();
                if (!tokenExp.CanSpawn(status))
                {
                    excluded.Add(tokenDef);
                }
            }
        }

        return excluded;
    }

    private bool CreateToken_Prefix(int col, bool noMatch)
    {
        var status = _status;
        if (status == null || status.isEmpty) return true;

        var targetSlot = GetLowestEmptySlot(col);
        if (targetSlot == null) return false;

        var token = UnityEngine.Object.Instantiate(_core.tokenPrefab, _core.tokenContainer, false);
        token.rectTransform.position = _core.puzzleSpawners[col].position;
        _newTokenCount++;
        targetSlot.SetToken(token, _newTokenCount * 0.025f);

        var excludeTokenDefs = GetTotalExcludedTokenDefs(status);

        if (!_initialRoundSettleComplete
            && targetSlot.row >= 5
            && !excludeTokenDefs.Contains(Game.Session.Puzzle.noSpawnMatchTokenDefinition))
        {
            excludeTokenDefs.Add(Game.Session.Puzzle.noSpawnMatchTokenDefinition);
        }

        int selectedTokenStatus = 0;
        if (_core.preloadedTokens.Count > col && _core.preloadedTokens[col].Count > 0)
        {
            var tokenDefinition = _core.preloadedTokens[col][_core.preloadedTokens[col].Count - 1];
            _core.preloadedTokens[col].RemoveAt(_core.preloadedTokens[col].Count - 1);

            if (tokenDefinition != null)
            {
                token.Define(tokenDefinition, true);
                status.GetTokenInfoByDefinition(tokenDefinition)?.AdjustCurrentWeight(-1);
                return false;
            }
        }

        do {
            int totalWeight = 0;
            foreach (var t in status.tokenStatus)
            {
                if (!excludeTokenDefs.Contains(t.tokenDefinition))
                    totalWeight += t.GetCurrentWeight();
            }

            // FIX 2: If all tokens are excluded or weights sum to <= 0, clear exclusions as fallback
            if (totalWeight <= 0)
            {
                throw new Exception("Invalid weight total of 0, cannot handle token spawning");
                // excludeTokenDefs.Clear();
                // totalWeight = status.tokenStatus.Sum(t => t.GetCurrentWeight());
            }

            int selectedWeight = UnityEngine.Random.Range(1, totalWeight + 1);
            int currentWeight = 0;
            for (int j = 0; j < status.tokenStatus.Count; j++)
            {
                if (excludeTokenDefs.Contains(status.tokenStatus[j].tokenDefinition)) continue;

                currentWeight += status.tokenStatus[j].GetCurrentWeight();
                if (selectedWeight <= currentWeight)
                {
                    selectedTokenStatus = j;
                    break;
                }
            }

            var tokenDef = status.tokenStatus[selectedTokenStatus].tokenDefinition;
            excludeTokenDefs.Add(tokenDef);
            token.Define(tokenDef, true);
        } 
        while (excludeTokenDefs.Count < status.tokenStatus.Count
            && (noMatch
                || !_initialRoundSettleComplete
                || token.definition == status.girlStatusFocused.noSpawnMatchTokenDef
                || status.girlStatusFocused.extraNoSpawnMatchTokenDefs.Contains(token.definition))
            && GetMatchWithSlot(targetSlot, false, 3) != null
        );

        status.tokenStatus[selectedTokenStatus].AdjustCurrentWeight(-1);
        return false;
    }
}
