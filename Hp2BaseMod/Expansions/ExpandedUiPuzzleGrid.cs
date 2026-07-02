using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
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
    {
        if (ExpandedUiPuzzleGrid.Get(__instance).SuppressFocusSwitch)
        {
            __result = false;
            return false;
        }
        return true;
    }

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
public class ExpandedUiPuzzleGrid
{
    private static readonly Dictionary<UiPuzzleGrid, ExpandedUiPuzzleGrid> _expansions
        = new Dictionary<UiPuzzleGrid, ExpandedUiPuzzleGrid>();

    /// <summary>
    /// The instance currently executing ConsumePuzzleSet.
    /// Non-null only for the duration of that call.
    /// </summary>
    public static ExpandedUiPuzzleGrid Current { get; private set; }

    public static ExpandedUiPuzzleGrid Get() => Get(Game.Session.Puzzle.puzzleGrid);
    public static ExpandedUiPuzzleGrid Get(UiPuzzleGrid core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiPuzzleGrid(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    // UiPuzzleGrid private field/method accessors
    private static readonly FieldInfo f_status =
        AccessTools.Field(typeof(UiPuzzleGrid), "_status");
    private static readonly FieldInfo f_state =
        AccessTools.Field(typeof(UiPuzzleGrid), "_state");
    private static readonly FieldInfo f_moveMatchSet =
        AccessTools.Field(typeof(UiPuzzleGrid), "_moveMatchSet");
    private static readonly FieldInfo f_moveSlotFrom =
        AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlotFrom");
    private static readonly FieldInfo f_moveSlotTo =
        AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlotTo");
    private static readonly FieldInfo f_moveSlots =
        AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlots");
    private static readonly FieldInfo f_moveHasBeenMade =
        AccessTools.Field(typeof(UiPuzzleGrid), "_moveHasBeenMade");
    private static readonly FieldInfo f_warningCheck =
        AccessTools.Field(typeof(UiPuzzleGrid), "_warningCheck");
    private static readonly FieldInfo f_resetTweener =
        AccessTools.Field(typeof(UiPuzzleGrid), "_resetTweener");
    private static readonly FieldInfo f_isResetting =
        AccessTools.Field(typeof(UiPuzzleGrid), "_isResetting");
    private static readonly FieldInfo f_energyTrails =
        AccessTools.Field(typeof(UiPuzzleGrid), "_energyTrails");

    private static readonly MethodInfo m_warningTooltip =
        AccessTools.Method(typeof(UiPuzzleGrid), "WarningTooltip");
    private static readonly MethodInfo m_clearMoveSlots =
        AccessTools.Method(typeof(UiPuzzleGrid), "ClearMoveSlots");
    private static readonly MethodInfo m_consumePuzzleSet =
        AccessTools.Method(typeof(UiPuzzleGrid), "ConsumePuzzleSet");
    private static readonly MethodInfo m_changeState =
        AccessTools.Method(typeof(UiPuzzleGrid), "ChangeState");
    private static readonly MethodInfo m_onResetAnimationComplete =
        AccessTools.Method(typeof(UiPuzzleGrid), "OnResetAnimationComplete");

    // PuzzleStatusGirl private field accessors — for exhaustion reversion
    private static readonly FieldInfo f_stamina =
        AccessTools.Field(typeof(PuzzleStatusGirl), "_stamina");
    private static readonly FieldInfo f_exhausted =
        AccessTools.Field(typeof(PuzzleStatusGirl), "_exhausted");
    private static readonly FieldInfo f_upset =
        AccessTools.Field(typeof(PuzzleStatusGirl), "_upset");

    private readonly UiPuzzleGrid _core;

    // Active consume context — stack-lifetime, set in ConsumePuzzleSet prefix.
    private PuzzleConsumeContext _activeConsumeContext;

    /// <summary>
    /// When true, AttemptGirlFocusSwitch returns false without switching.
    /// </summary>
    public bool SuppressFocusSwitch { get; set; }

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
    private readonly List<IUiPuzzleGridModifier> _pendingModifiers
        = new List<IUiPuzzleGridModifier>();
    private readonly List<IUiPuzzleGridModifier> _activeModifiers
        = new List<IUiPuzzleGridModifier>();

    /// <summary>
    /// Registers a modifier to be applied when the next puzzle starts.
    /// If called after StartPuzzle has already run, the modifier is applied immediately.
    /// Safe to call multiple times with the same instance — duplicates are ignored.
    /// </summary>
    public void AddModifier(IUiPuzzleGridModifier modifier)
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
    public void RemoveModifier(IUiPuzzleGridModifier modifier)
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

    private ExpandedUiPuzzleGrid(UiPuzzleGrid core)
    {
        _core = core;
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

    /// <summary>
    /// When SuppressStaminaCost is active and the game is in MOVING state on mouse-up,
    /// reproduces the original move processing loop without stamina deduction,
    /// stamina sufficiency check, or unfocused stamina recovery.
    /// Returns false to skip the original Update only when we handle the move.
    /// </summary>
    public bool Update()
    {
        if (!SuppressStaminaCost)
        {
            return true;
        }

        if (!Game.Session.Location.AtLocationType(LocationType.DATE)
            || !Game.Session.Puzzle.isPuzzleActive
            || Game.Manager.Time.IsPaused(_core.pauseDefinition)
            || !Input.GetMouseButtonUp(0))
        {
            return true;
        }

        var state = (PuzzleGameState)f_state.GetValue(_core);
        if (state != PuzzleGameState.MOVING)
        {
            return true;
        }

        m_warningTooltip.Invoke(_core, new object[] { null });
        _core.guideContainer.HideMovementGuides();

        var status       = f_status.GetValue(_core) as PuzzleStatus;
        var moveMatchSet = f_moveMatchSet.GetValue(_core) as PuzzleSet;
        var moveSlotFrom = f_moveSlotFrom.GetValue(_core) as UiPuzzleSlot;
        var moveSlotTo   = f_moveSlotTo.GetValue(_core) as UiPuzzleSlot;

        var girlStatusFocused   = status.girlStatusFocused;
        var girlStatusUnfocused = status.girlStatusUnfocused;

        // Stamina sufficiency check is skipped — any move is valid.
        bool validMove = moveMatchSet != null
            && Game.Session.Puzzle.TutorialStepCheck(moveSlotFrom, moveSlotTo);

        if (validMove)
        {
            int movesCost = moveMatchSet.GetMovesCost();
            int staminaCostRawFull = moveMatchSet.GetStaminaCost(true, true);

            Game.Session.gameCanvas.effectsContainerLow.dragCursor.Deactivate(null);
            moveMatchSet.DimTokens();
            moveSlotFrom.token.Show();

            f_moveSlotFrom.SetValue(_core, null);

            var moveSlots = f_moveSlots.GetValue(_core) as List<UiPuzzleSlot>;
            for (int i = 0; i < moveSlots.Count; i++)
            {
                moveSlots[i].ApplyTempToken();
            }

            var moveModifier = Game.Session.Ailment.Trigger(moveMatchSet);
            f_moveHasBeenMade.SetValue(_core, true);

            bool isBigMatch = (bool)m_consumePuzzleSet.Invoke(_core, new object[] { moveMatchSet, true });

            if (!status.bonusRound)
            {
                // Apply move cost — still respected even without stamina cost.
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

            f_moveMatchSet.SetValue(_core, null);
            m_clearMoveSlots.Invoke(_core, null);
        }
        else
        {
            // Invalid move - mirror original reset path exactly.
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

            f_moveMatchSet.SetValue(_core, null);
            m_clearMoveSlots.Invoke(_core, null);

            var resetTweener = Game.Session.gameCanvas.effectsContainerLow.dragCursor.rectTransform
                .DOMove(moveSlotFrom.rectTransform.position, 0.25f, false)
                .SetEase(Ease.OutSine)
                .OnComplete(() => m_onResetAnimationComplete.Invoke(_core, null));

            f_resetTweener.SetValue(_core, resetTweener);
            Game.Manager.Time.Play(resetTweener, _core.pauseDefinition, 0f);
            f_isResetting.SetValue(_core, true);
            m_changeState.Invoke(_core, new object[] { PuzzleGameState.RESETTING });
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

        var state = (PuzzleGameState)f_state.GetValue(_core);
        if (state != PuzzleGameState.MOVING)
        {
            return;
        }

        var moveMatchSet = f_moveMatchSet.GetValue(_core) as PuzzleSet;
        if (moveMatchSet == null)
        {
            return;
        }

        var status = f_status.GetValue(_core) as PuzzleStatus;

        if (SuppressStaminaWarning
            && status.girlStatusFocused.stamina < moveMatchSet.GetStaminaCost(false, false))
        {
            f_warningCheck.SetValue(_core, false);
        }
        else if (SuppressUpsetWarning
            && (moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.noSpawnMatchTokenDef)
                || moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.extraNoSpawnMatchTokenDefs))
            && !status.girlStatusFocused.HasAilment(Game.Session.Puzzle.brokenProtectionAilmentDefinition, true))
        {
            f_warningCheck.SetValue(_core, false);
        }
        else if (SuppressExhaustionWarning
            && status.girlStatusFocused.stamina == moveMatchSet.GetStaminaCost(false, false))
        {
            f_warningCheck.SetValue(_core, false);
        }
    }

    public void OnResourceChanged()
    {
        if (!SuppressStaminaCost)
        {
            return;
        }

        var status = f_status.GetValue(_core) as PuzzleStatus;

        // Silently revert exhaustion and upset on both girls when stamina is suppressed.
        // Baggage triggers are still allowed to fire (they happen before resource change propagates), 
        // but the exhausted/upset state is cleared immediately after.
        RevertExhaustion(status.girlStatusLeft);
        RevertExhaustion(status.girlStatusRight);
    }

    private void RevertExhaustion(PuzzleStatusGirl girl)
    {
        if (!girl.exhausted && !girl.upset)
        {
            return;
        }

        f_stamina.SetValue(girl, 6);
        f_exhausted.SetValue(girl, false);
        f_upset.SetValue(girl, false);
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
        var status  = f_status.GetValue(_core) as PuzzleStatus;

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
        var status = f_status.GetValue(_core) as PuzzleStatus;

        // Stage 0: Setup
        MatchModifier modifier = Game.Session.Ailment.Trigger(match);

        if (modifier.tokenDefinition != null)
        {
            match.tokenDefinition = modifier.tokenDefinition;
        }

        if (modifier.absorb)
        {
            altGirl = modifier.absorbAltGirl;
        }

        PuzzleStatusGirl girl = status.GetStatusGirl(altGirl);
        Game.Persistence.playerFile.GetPlayerFileGirl(girl.girlDefinition);

        List<UiPuzzleSlot> slots = (
            from slot in ListUtils.CopyList(match.slots)
            orderby slot.row + slot.col, UnityEngine.Random.Range(0f, 1f)
            select slot).ToList();

        List<UiPuzzleSlot> ordered = new List<UiPuzzleSlot>();
        while (slots.Count > 0)
        {
            float center = (slots.Count - 1) * 0.5f;
            int index = Mathf.Clamp(
                MathUtils.RandomBool() ? Mathf.FloorToInt(center) : Mathf.CeilToInt(center),
                0, slots.Count - 1);
            ordered.Add(slots[index]);
            slots.RemoveAt(index);
        }

        // Stage 1: Annotation
        var context = new PuzzleRewardContext(match, altGirl, modifier, ordered);

        bool isAffection = match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION;
        bool isBoss = girl.girlDefinition.bossCharacter;

        context.Properties.FlatMatch = match.flatMatch;
        context.Properties.IsBossCharacter = isBoss;
        context.Properties.SkipMostFavFactor = modifier.skipMostFavFactor;
        context.Properties.SkipLeastFavFactor = modifier.skipLeastFavFactor;

        if (isAffection && !isBoss)
        {
            context.Properties.IsMostFav = !modifier.skipMostFavFactor
                && match.tokenDefinition.affectionType == girl.girlDefinition.GetMostFavAffectionType();

            context.Properties.IsLeastFav = !modifier.skipLeastFavFactor
                && match.tokenDefinition.affectionType == girl.girlDefinition.GetLeastFavAffectionType();
        }

        DispatchToAllAilments(status, (scripted, ailment, targetGirl) =>
            scripted.OnPreMatchReward(ailment, targetGirl, context));

        // Stage 2: Formula
        int value = ordered.Count;

        if (!status.bonusRound)
        {
            switch (match.tokenDefinition.resourceType)
            {
                case PuzzleResourceType.AFFECTION:
                {
                    if (!context.Properties.FlatMatch)
                    {
                        value *= Mathf.Max(ordered.Count - 2, 1);
                    }

                    // Both IsMostFav and IsLeastFav true → normal (2x) multiplier.
                    bool mostFav = context.Properties.IsMostFav
                        && !context.Properties.IsLeastFav
                        && !context.Properties.IsBossCharacter
                        && !context.Properties.SkipMostFavFactor;

                    bool leastFav = context.Properties.IsLeastFav
                        && !context.Properties.IsMostFav
                        && !context.Properties.IsBossCharacter
                        && !context.Properties.SkipLeastFavFactor;

                    if (mostFav)
                    {
                        value *= 3;
                    }
                    else if (!leastFav)
                    {
                        value *= 2;
                    }

                    int multiplier = 0;
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        if (ordered[i].token.upgraded)
                        {
                            multiplier += Mathf.Max(3 + Game.Session.Puzzle.GetPuzzleOffset("power_token_multiplier"), 0);
                        }
                    }

                    if (multiplier > 0)
                    {
                        value *= multiplier;
                    }

                    value += Mathf.RoundToInt(value * (4f * (Game.Persistence.playerFile.GetAffectionLevelExp(match.tokenDefinition.affectionType, false) / 24f)));

                    value += Mathf.RoundToInt(value * (Game.Persistence.playerFile.passionMultiplier * (girl.passion * 0.01f)));

                    break;
                }

                case PuzzleResourceType.MOVES:
                    value = Mathf.Max(ordered.Count - 2, 0);
                    break;

                case PuzzleResourceType.PASSION:
                    if (!context.Properties.FlatMatch)
                    {
                        value *= Mathf.Max(ordered.Count - 2, 1);
                    }

                    value *= 5;
                    break;

                case PuzzleResourceType.SENTIMENT:
                    if (!context.Properties.FlatMatch)
                    {
                        value *= Mathf.Max(ordered.Count - 2, 1);
                    }

                    break;
            }

            if (modifier.pointsOp)
            {
                value = Mathf.RoundToInt(MathUtils.CombineValues(
                    modifier.pointsOperation, value, modifier.pointsFactor));
            }

            if (modifier.pointsOp2)
            {
                value = Mathf.RoundToInt(MathUtils.CombineValues(
                    modifier.pointsOperation2, value, modifier.pointsFactor2));
            }

            if (match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION && value >= 5000)
            {
                Game.Manager.Platform.UnlockAchievement("smooth_move", true);
            }
        }
        else
        {
            value *= Mathf.Max(ordered.Count - 2, 1);
            value *= 10;
        }

        // Stage 3: Modification
        if (context.CancelRewards)
        {
            value = 0;
        }

        value += context.RewardBonus;

        bool negative = value < 0;
        int total = Mathf.Abs(value);
        value = total;

        var rewards = new Dictionary<UiPuzzleSlot, PuzzleReward>();

        for (int i = 0; i < ordered.Count; i++)
        {
            int amount = Mathf.CeilToInt(value / (float)(ordered.Count - i));

            if (match.tokenDefinition.resourceType == PuzzleResourceType.BROKEN)
            {
                amount = value;
            }

            value -= amount;

            var reward = new PuzzleReward(
                match.tokenDefinition,
                negative ? -amount : amount,
                altGirl);

            if (total == 0)
            {
                reward.zeroedValue = true;
            }

            if (!status.bonusRound && !status.IsTutorial(false) && !match.flatMatch && i == 0)
            {
                if (modifier.replaceDefinition != null && modifier.replacePriority)
                {
                    reward.replaceDefinition = modifier.replaceDefinition;
                }

                if (reward.replaceDefinition == null
                    && match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION
                    && ordered.Count > 2
                    && total > 0)
                {
                    float chance;

                    switch (ordered.Count)
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

                    if (chance == 1 || UnityEngine.Random.Range(0f, 1f) <= chance)
                    {
                        reward.replaceDefinition = match.tokenDefinition;
                        reward.replaceUpgraded = true;
                    }
                }

                if (reward.replaceDefinition == null
                    && modifier.replaceDefinition != null
                    && !modifier.replacePriority)
                {
                    reward.replaceDefinition = modifier.replaceDefinition;
                }
            }

            rewards.Add(ordered[i], reward);
        }

        var result = rewards;

        DispatchToAllAilments(status, (scripted, ailment, targetGirl) =>
            scripted.OnPostMatchReward(ailment, targetGirl, context, result));

        rewards = result;

        ListUtils.DictionaryAddRangeUnique(_activeConsumeContext.Rewards, rewards);
        _activeConsumeContext.AdditionalRewards.AddRange(context.AdditionalRewards);

        return rewards;
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

        for (int i = 0; i < ordered.Count; i++)
        {
            var (ailment, girl) = ordered[i];
            if (!ailment.isEnabled) continue;

            var scripted = ailment.Expansion().ScriptedAilment;
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

        ProcessGirl(status.girlStatusFocused,   firstPriority, lastPriority, first, baggage, normal, last);
        ProcessGirl(status.girlStatusUnfocused, firstPriority, lastPriority, first, baggage, normal, last);

        var result = new List<(Ailment, PuzzleStatusGirl)>(first.Count + baggage.Count + normal.Count + last.Count);
        result.AddRange(first);
        result.AddRange(baggage);
        result.AddRange(normal);
        result.AddRange(last);
        return result;
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

        for (int i = 0; i < girl.ailments.Count; i++)
        {
            var ailment = girl.ailments[i];

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