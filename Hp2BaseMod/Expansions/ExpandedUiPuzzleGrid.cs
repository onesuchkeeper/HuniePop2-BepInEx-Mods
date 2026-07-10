using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiPuzzleGrid))]
internal static class UiPuzzleGridPatch
{
    [HarmonyPatch("ConsumePuzzleSet")]
    [HarmonyPrefix]
    public static bool ConsumePuzzleSet_Prefix(
        UiPuzzleGrid __instance,
        PuzzleSet puzzleSet,
        bool andDestroy,
        ref bool __result)
        => ExpandedUiPuzzleGrid.Get(__instance).ConsumePuzzleSet_Prefix(puzzleSet, andDestroy, ref __result);

    [HarmonyPatch("ConsumePuzzleSet")]
    [HarmonyPostfix]
    public static void ConsumePuzzleSet_Postfix(
        UiPuzzleGrid __instance,
        PuzzleSet puzzleSet,
        bool andDestroy,
        ref bool __result)
        => ExpandedUiPuzzleGrid.Get(__instance).ConsumePuzzleSet_Postfix(ref __result);

    [HarmonyPatch("AttemptGirlFocusSwitch")]
    [HarmonyPrefix]
    public static bool AttemptGirlFocusSwitch(UiPuzzleGrid __instance, ref bool __result)
        => ExpandedUiPuzzleGrid.Get(__instance).AttemptGirlFocusSwitch(ref __result);

    /// <summary>
    /// Intercepts MOVING state mouse-up when SuppressStaminaCost is active.
    /// Reproduces the original move processing loop without stamina deduction,
    /// stamina sufficiency check, or unfocused stamina recovery.
    /// Returns false (skip original) only when we handle the move ourselves.
    /// </summary>
    [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
    [HarmonyPostfix]
    public static void StartPuzzle(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).StartPuzzle();

    [HarmonyPatch(nameof(UiPuzzleGrid.EndPuzzle))]
    [HarmonyPostfix]
    public static void EndPuzzle(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).EndPuzzle();

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    public static bool Update(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).Update();

    [HarmonyPatch("OnSlotEnter")]
    [HarmonyPostfix]
    public static void OnSlotEnter(UiPuzzleGrid __instance, UiPuzzleSlot slot)
        => ExpandedUiPuzzleGrid.Get(__instance).OnSlotEnter();

    [HarmonyPatch("OnResourceChanged")]
    [HarmonyPostfix]
    public static void OnResourceChanged(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).OnResourceChanged();

    [HarmonyPatch("CheckRoundOver")]
    [HarmonyPostfix]
    public static void CheckRoundOver(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).CheckRoundOver();

    [HarmonyPatch("StartNewRound")]
    [HarmonyPostfix]
    public static void StartNewRound(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).StartNewRound();
}

[HarmonyPatch(typeof(PuzzleSet))]
internal static class PuzzleSetPatch
{
    /// <summary>
    /// Replaces GetMatchRewards entirely. Returns false to skip the original.
    /// All reward calculation now runs through the scripted pipeline.
    /// </summary>
    [HarmonyPatch("GetMatchRewards")]
    [HarmonyPrefix]
    public static bool GetMatchRewards_Prefix(
        PuzzleSet __instance,
        PuzzleMatch match,
        bool altGirl,
        ref Dictionary<UiPuzzleSlot, PuzzleReward> __result)
    {
        if (ExpandedUiPuzzleGrid.Current == null)
        {
            return true;
        }

        __result = ExpandedUiPuzzleGrid.Current.ExecuteGetMatchRewards(__instance, match, altGirl);
        return false;
    }
}

/// <summary>
/// Companion class for <see cref="UiPuzzleGrid"/> that replaces the reward calculation
/// pipeline with a scripted-ailment-aware version and exposes flags for ailments to
/// control grid behaviour that was previously hardcoded.
/// </summary>
[Expansion(typeof(UiPuzzleGrid), 
    Fields = new[]{"_status", "_state", "_moveMatchSet", "_moveSlotFrom", "_moveSlotTo", "_moveSlots", "_moveHasBeenMade",
        "_warningCheck", "_resetTweener", "_isResetting", "_energyTrails", "_roundState", "_roundOver"},
    Methods = new[]{"WarningTooltip", "ClearMoveSlots", "OnResetAnimationComplete"})]
public partial class ExpandedUiPuzzleGrid
{
    public static ExpandedUiPuzzleGrid Get() => Get(Game.Session.Puzzle.puzzleGrid);

    /// <summary>
    /// The instance currently executing ConsumePuzzleSet.
    /// Non-null only for the duration of that call.
    /// </summary>
    public static ExpandedUiPuzzleGrid Current { get; private set; }

    public PuzzleRoundState RoundState
    {
        get => _roundState;
        set => _roundState = value;
    }

    // Active consume context, stack-lifetime, set in ConsumePuzzleSet prefix.
    private PuzzleConsumeContext _activeConsumeContext;

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
    /// True when focus switching is currently suppressed by the counter or
    /// by any active modifier.
    /// </summary>
    public bool IsFocusSwitchSuppressed => _suppressFocusSwitchCount > 0;

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

    // Modifiers registered before StartPuzzle and applied/removed automatically.
    private readonly List<IPuzzleGridModifier> _pendingModifiers
        = new List<IPuzzleGridModifier>();
    private readonly List<IPuzzleGridModifier> _activeModifiers
        = new List<IPuzzleGridModifier>();

    /// <summary>
    /// Registers a modifier to be applied when the next puzzle starts.
    /// If called after StartPuzzle has already run, the modifier is applied immediately.
    /// Safe to call multiple times with the same instance, duplicates are ignored.
    /// </summary>
    public void AddModifier(IPuzzleGridModifier modifier)
    {
        if (_activeModifiers.Contains(modifier) || _pendingModifiers.Contains(modifier))
        {
            return;
        }

        // If the puzzle is already running, apply immediately.
        var status = f_status.GetValue(_core) as PuzzleStatus;
        if (status != null && !status.isEmpty && Game.Session.Puzzle.isPuzzleActive)
        {
            _activeModifiers.Add(modifier);
            modifier.OnApply(_core, this, status);
        }
        else
        {
            _pendingModifiers.Add(modifier);
        }
    }

    /// <summary>
    /// Removes a modifier. If the puzzle is active, OnRemove is called immediately.
    /// If the modifier is still pending (puzzle not yet started), it is simply discarded.
    /// </summary>
    public void RemoveModifier(IPuzzleGridModifier modifier)
    {
        if (_pendingModifiers.Remove(modifier))
        {
            return;
        }

        if (_activeModifiers.Remove(modifier))
        {
            var status = f_status.GetValue(_core) as PuzzleStatus;
            modifier.OnRemove(_core, this, status);
        }
    }

    public void StartPuzzle()
    {
        var status = f_status.GetValue(_core) as PuzzleStatus;

        // Apply all pending modifiers in registration order.
        for (int i = 0; i < _pendingModifiers.Count; i++)
        {
            _activeModifiers.Add(_pendingModifiers[i]);
            _pendingModifiers[i].OnApply(_core, this, status);
        }
        _pendingModifiers.Clear();
    }

    public void EndPuzzle()
    {
        var status = f_status.GetValue(_core) as PuzzleStatus;

        // Remove all active modifiers in reverse order.
        for (int i = _activeModifiers.Count - 1; i >= 0; i--)
        {
            _activeModifiers[i].OnRemove(_core, this, status);
        }
        _activeModifiers.Clear();
    }

    public bool AttemptGirlFocusSwitch(ref bool __result)
    {
        if (IsFocusSwitchSuppressed) 
        {
            __result = false;
            return false;
        }

        var status = f_status.GetValue(_core) as PuzzleStatus;

        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            if (!_activeModifiers[i].OnAttemptFocusSwitch(status)) 
            {
                __result = false;
                return false;
            }
        }

        return true;
    }

    public bool CanEnableAilment(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            if (!_activeModifiers[i].CanEnableAilment(ailment, girl, otherGirl))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Called from the AilmentManager.TriggerAilment postfix.
    /// Retrieves the current move/match context from AilmentManager private fields
    /// so modifiers receive the same data ailments do.
    /// </summary>
    internal void OnTrigger(AilmentTriggerType triggerType)
    {
        if (_activeModifiers.Count == 0) return;

        var status = f_status.GetValue<PuzzleStatus>(_core);

        var ailmentManager = Game.Session.Ailment.GetExpansion();

        var move = ailmentManager.Move;
        var moveModifier = ailmentManager.MoveModifier;
        var match = ailmentManager.Match;
        var matchModifier = ailmentManager.MatchModifier;

        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            _activeModifiers[i].OnTrigger(triggerType, move, moveModifier, match, matchModifier, status);
        }
    }

    public void StartNewRound()
    {
        if (_activeModifiers.Count == 0) return;

        var status = f_status.GetValue(_core) as PuzzleStatus;

        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            _activeModifiers[i].OnRoundStart(status);
        }
    }

    public void CheckRoundOver()
    {
        if (_activeModifiers.Count == 0) return;

        var status = f_status.GetValue(_core) as PuzzleStatus;
        var statusExp = status.GetExpansion();
        var roundState = (PuzzleRoundState)f_roundState.GetValue(_core);
        bool roundOver = (bool)f_roundOver.GetValue(_core);

        // Only dispatch when the round actually ended this frame.
        if (!roundOver) return;

        bool isSuccess = roundState == PuzzleRoundState.SUCCESS;
        bool isGameOver = status.gameOver;
        var context = new PuzzleRoundContext(isSuccess, isGameOver, status.bonusRound, status.statusType);

        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            _activeModifiers[i].OnRoundEnd(context, status);
        }

        _roundState = context.IsSuccess ? PuzzleRoundState.SUCCESS : PuzzleRoundState.FAILURE;
        statusExp.GameOver = context.IsGameOver;
    }

    /// <summary>
    /// When SuppressStaminaCost is active and the game is in MOVING state on mouse-up,
    /// reproduces the original move processing loop without stamina deduction,
    /// stamina sufficiency check, or unfocused stamina recovery.
    /// Returns false to skip the original Update only when we handle the move.
    /// </summary>
    public bool Update()
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

    public void OnSlotEnter()
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

    public void OnResourceChanged()
    {
        var status = f_status.GetValue(_core) as PuzzleStatus;

        for (int i = 0; i < _activeModifiers.Count; i++) 
        {
            _activeModifiers[i].OnResourceChanged(status);
        }
    }

    /// <summary>
    /// Silently reverts exhaustion and upset on a girl, setting her stamina to max.
    /// Call from <see cref="IUiPuzzleGridModifier.OnResourceChanged"/> when your
    /// modifier needs to prevent exhaustion from persisting - e.g. when stamina costs
    /// are suppressed and baggage triggers have already fired but the state should not stick.
    /// </summary>
    public void RevertExhaustion(PuzzleStatusGirl girl)
    {
        if (!girl.exhausted && !girl.upset) return;
        
        var girlStatusExp = girl.GetExpansion();
        girlStatusExp.Stamina = 6;
        girlStatusExp.Exhausted = false;
        girlStatusExp.Upset = false;
    }

    public bool ConsumePuzzleSet_Prefix(PuzzleSet puzzleSet, bool andDestroy, ref bool __result)
    {
        _activeConsumeContext = new PuzzleConsumeContext(puzzleSet, andDestroy);
        Current = this;
        return true;
    }

    public void ConsumePuzzleSet_Postfix(ref bool __result)
    {
        if (_activeConsumeContext == null)
        {
            Current = null;
            return;
        }

        var context = _activeConsumeContext;
        var status = f_status.GetValue(_core) as PuzzleStatus;

        DispatchToAllAilments(status, (scripted, ailment, girl) =>
            scripted.OnPostSetReward(ailment, girl, context));

        if (!context.CancelConsume)
        {
            var trails = f_energyTrails.GetValue(_core) as List<EnergyTrailBehavior>;

            for (int i = 0; i < context.AdditionalRewards.Count; i++)
            {
                var (slot, reward) = context.AdditionalRewards[i];
                status.AddPuzzleReward(reward);

                var trail = UnityEngine.Object.Instantiate(_core.energyTrailPrefab);
                trails?.Add(trail);
                trail.Init(
                    reward.zeroedValue ? EnergyTrailFormat.START : EnergyTrailFormat.FULL,
                    reward,
                    slot);
            }

            status.CheckChanges();
        }

        _activeConsumeContext = null;
        Current = null;
    }

    public Dictionary<UiPuzzleSlot, PuzzleReward> ExecuteGetMatchRewards(
        PuzzleSet puzzleSet,
        PuzzleMatch match,
        bool altGirl)
    {
        ModInterface.Log.Message();
        var status = f_status.GetValue(_core) as PuzzleStatus;

        // Stage 0: Setup
        var matchModifier = Game.Session.Ailment.Trigger(match);

        if (matchModifier.tokenDefinition != null)
        {
            match.tokenDefinition = matchModifier.tokenDefinition;
        }

        if (matchModifier.absorb)
        {
            altGirl = matchModifier.absorbAltGirl;
        }

        var statusGirl = status.GetStatusGirl(altGirl);
        Game.Persistence.playerFile.GetPlayerFileGirl(statusGirl.girlDefinition);

        var orderedSlots = match.slots
            .OrderBy(slot => slot.row + slot.col)
            .ThenBy(_ => UnityEngine.Random.Range(0f, 1f))
            .ToList();

        var sortedSlots = new List<UiPuzzleSlot>();

        while (orderedSlots.Count > 0)
        {
            var centerIndex = (orderedSlots.Count - 1) * 0.5f;

            var selectedIndex = Mathf.Clamp(
                MathUtils.RandomBool()
                    ? Mathf.FloorToInt(centerIndex)
                    : Mathf.CeilToInt(centerIndex),
                0,
                orderedSlots.Count - 1);

            sortedSlots.Add(orderedSlots[selectedIndex]);
            orderedSlots.RemoveAt(selectedIndex);
        }

        // Stage 1: Annotation
        var context = new PuzzleRewardContext(match, altGirl, matchModifier, sortedSlots);

        bool isAffection = match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION;
        bool isBoss = statusGirl.girlDefinition.bossCharacter;

        context.Properties.FlatMatch = match.flatMatch;
        context.Properties.IsBossCharacter = isBoss;
        context.Properties.SkipMostFavFactor = matchModifier.skipMostFavFactor;
        context.Properties.SkipLeastFavFactor = matchModifier.skipLeastFavFactor;

        if (isAffection && !isBoss)
        {
            context.Properties.IsMostFav = !matchModifier.skipMostFavFactor
                && match.tokenDefinition.affectionType == statusGirl.girlDefinition.GetMostFavAffectionType();

            context.Properties.IsLeastFav = !matchModifier.skipLeastFavFactor
                && match.tokenDefinition.affectionType == statusGirl.girlDefinition.GetLeastFavAffectionType();
        }

        DispatchToAllAilments(status, (scripted, ailment, girl) =>
            scripted.OnPreMatchReward(ailment, girl, context));

        // Stage 2: Formula
        int totalRewardAmount = sortedSlots.Count;

        if (!status.bonusRound)
        {
            switch (match.tokenDefinition.resourceType)
            {
                case PuzzleResourceType.AFFECTION:
                {
                    if (!context.Properties.FlatMatch)
                    {
                        totalRewardAmount *= Mathf.Max(sortedSlots.Count - 2, 1);
                    }

                    // Both IsMostFav and IsLeastFav true use normal (2x) multiplier.
                    bool effectiveMostFav = context.Properties.IsMostFav
                        && !context.Properties.IsLeastFav
                        && !context.Properties.IsBossCharacter
                        && !context.Properties.SkipMostFavFactor;

                    bool effectiveLeastFav = context.Properties.IsLeastFav
                        && !context.Properties.IsMostFav
                        && !context.Properties.IsBossCharacter
                        && !context.Properties.SkipLeastFavFactor;

                    if (effectiveMostFav)
                    {
                        totalRewardAmount *= 3;
                    }
                    else if (!effectiveLeastFav)
                    {
                        totalRewardAmount *= 2;
                    }

                    int mult = 0;
                    for (int i = 0; i < sortedSlots.Count; i++)
                    {
                        if (sortedSlots[i].token.upgraded)
                        {
                            mult += Mathf.Max(
                                3 + Game.Session.Puzzle.GetPuzzleOffset("power_token_multiplier"), 0);
                        }
                    }
                    if (mult > 0) totalRewardAmount *= mult;

                    totalRewardAmount += Mathf.RoundToInt(totalRewardAmount
                        * (4f * (Game.Persistence.playerFile.GetAffectionLevelExp(
                            match.tokenDefinition.affectionType, false) / 24f)));

                    totalRewardAmount += Mathf.RoundToInt(totalRewardAmount
                        * (Game.Persistence.playerFile.passionMultiplier * (statusGirl.passion * 0.01f)));

                    break;
                }

                case PuzzleResourceType.MOVES:
                    totalRewardAmount = Mathf.Max(sortedSlots.Count - 2, 0);
                    break;

                case PuzzleResourceType.PASSION:
                    if (!context.Properties.FlatMatch) totalRewardAmount *= Mathf.Max(sortedSlots.Count - 2, 1);
                    totalRewardAmount *= 5;
                    break;

                case PuzzleResourceType.SENTIMENT:
                    if (!context.Properties.FlatMatch) totalRewardAmount *= Mathf.Max(sortedSlots.Count - 2, 1);
                    break;
            }

            if (matchModifier.pointsOp)
            {
                totalRewardAmount = Mathf.RoundToInt(MathUtils.CombineValues(
                    matchModifier.pointsOperation, totalRewardAmount, matchModifier.pointsFactor));
            }

            if (matchModifier.pointsOp2)
            {
                totalRewardAmount = Mathf.RoundToInt(MathUtils.CombineValues(
                    matchModifier.pointsOperation2, totalRewardAmount, matchModifier.pointsFactor2));
            }

            if (match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION && totalRewardAmount >= 5000)
            {
                Game.Manager.Platform.UnlockAchievement("smooth_move", true);
            }
        }
        else
        {
            totalRewardAmount *= Mathf.Max(sortedSlots.Count - 2, 1);
            totalRewardAmount *= 10;
        }

        // Stage 3: Modification
        if (context.CancelRewards) totalRewardAmount = 0;
        totalRewardAmount += context.RewardBonus;

        bool flag = totalRewardAmount < 0;
        int absoluteRewardAmount = Mathf.Abs(totalRewardAmount);
        totalRewardAmount = absoluteRewardAmount;

        var dictionary = new Dictionary<UiPuzzleSlot, PuzzleReward>();

        for (int j = 0; j < sortedSlots.Count; j++)
        {
            var portion = Mathf.CeilToInt(totalRewardAmount / (float)(sortedSlots.Count - j));
            if (match.tokenDefinition.resourceType == PuzzleResourceType.BROKEN) portion = totalRewardAmount;
            totalRewardAmount -= portion;

            var reward = new PuzzleReward(
                match.tokenDefinition,
                flag ? -portion : portion,
                altGirl);

            if (absoluteRewardAmount == 0) reward.zeroedValue = true;

            if (!status.bonusRound && !status.IsTutorial(false) && !match.flatMatch && j == 0)
            {
                if (matchModifier.replaceDefinition != null && matchModifier.replacePriority)
                {
                    reward.replaceDefinition = matchModifier.replaceDefinition;
                }

                if (reward.replaceDefinition == null
                    && match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION
                    && sortedSlots.Count > 2
                    && absoluteRewardAmount > 0)
                {
                    float chance;
                    switch (sortedSlots.Count)
                    {
                        case 3:
                            chance = 0f + 0.1f * Game.Persistence.playerFile.styleFactor + Game.Session.Puzzle.GetPuzzleOffset("power_token_chance") * 0.025f; 
                            break;
                        case 4: 
                            chance = 0.2f + 0.6f * Game.Persistence.playerFile.styleFactor + Game.Session.Puzzle.GetPuzzleOffset("power_token_chance") * 0.15f; 
                            break;
                        case 5: 
                            chance = 0.8f + 0.2f * Game.Persistence.playerFile.styleFactor + Game.Session.Puzzle.GetPuzzleOffset("power_token_chance") * 0.05f; 
                            break;
                        default:
                            chance = 1f; 
                            break;
                    }

                    chance = Mathf.Clamp(chance, 0f, 1f);
                    if (UnityEngine.Random.Range(0f, 1f) <= chance)
                    {
                        reward.replaceDefinition = match.tokenDefinition;
                        reward.replaceUpgraded   = true;
                    }
                }

                if (reward.replaceDefinition == null
                    && matchModifier.replaceDefinition != null
                    && !matchModifier.replacePriority)
                {
                    reward.replaceDefinition = matchModifier.replaceDefinition;
                }
            }

            dictionary.Add(sortedSlots[j], reward);
        }

        var rewards = dictionary;
        DispatchToAllAilments(status, (scripted, ailment, girl) =>
            scripted.OnPostMatchReward(ailment, girl, context, rewards));
        dictionary = rewards;

        ListUtils.DictionaryAddRangeUnique(_activeConsumeContext.Rewards, dictionary);
        _activeConsumeContext.AdditionalRewards.AddRange(context.AdditionalRewards);

        return dictionary;
    }

    private void DispatchToAllAilments(
        PuzzleStatus status,
        Action<IScriptedAilment, Ailment, PuzzleStatusGirl> action)
    {
        if (status == null || status.isEmpty) return;

        var ordered = BuildOrderedAilmentList(
            status,
            Game.Session.Ailment.firstPriorityAilDefs,
            Game.Session.Ailment.lastPriorityAilDefs);

        foreach (var (ailment, girl) in ordered)
        {
            if (!ailment.isEnabled) continue;

            var scripted = ailment.GetExpansion().ScriptedAilment;
            if (scripted == null) continue;

            action(scripted, ailment, girl);
        }
    }

    private static List<(Ailment, PuzzleStatusGirl)> BuildOrderedAilmentList(
        PuzzleStatus status,
        List<AilmentDefinition> firstPriority,
        List<AilmentDefinition> lastPriority)
    {
        var first = new List<(Ailment, PuzzleStatusGirl)>();
        var baggage = new List<(Ailment, PuzzleStatusGirl)>();
        var normal = new List<(Ailment, PuzzleStatusGirl)>();
        var last = new List<(Ailment, PuzzleStatusGirl)>();

        ProcessGirl(status.girlStatusFocused, firstPriority, lastPriority, first, baggage, normal, last);
        ProcessGirl(status.girlStatusUnfocused, firstPriority, lastPriority, first, baggage, normal, last);

        return first.Concat(baggage).Concat(normal).Concat(last).ToList();
    }

    private static void ProcessGirl(
        PuzzleStatusGirl girl,
        List<AilmentDefinition> firstPriority,
        List<AilmentDefinition> lastPriority,
        List<(Ailment, PuzzleStatusGirl)> first,
        List<(Ailment, PuzzleStatusGirl)> baggage,
        List<(Ailment, PuzzleStatusGirl)> normal,
        List<(Ailment, PuzzleStatusGirl)> last)
    {
        if (girl == null) return;

        foreach(var ailment in girl.ailments)
        {
            if (firstPriority.Contains(ailment.definition))
            {
                int idx = 0;
                while (idx < first.Count
                    && firstPriority.IndexOf(ailment.definition)
                       > firstPriority.IndexOf(first[idx].Item1.definition))
                {
                    idx++;
                }
                    
                first.Insert(idx, (ailment, girl));
            }
            else if (lastPriority.Contains(ailment.definition))
            {
                int idx = 0;

                while (idx < last.Count
                    && lastPriority.IndexOf(ailment.definition)
                       > lastPriority.IndexOf(last[idx].Item1.definition))
                {
                    idx++;
                }
                    
                last.Insert(idx, (ailment, girl));
            }
            else if (girl.girlDefinition.baggageItemDefs.Contains(ailment.definition.itemDefinition))
            {
                baggage.Add((ailment, girl));
            }
            else
            {
                normal.Add((ailment, girl));
            }
        }
    }
}