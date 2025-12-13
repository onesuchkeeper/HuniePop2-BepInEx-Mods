using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleGrid))]
internal static class UiPuzzleGridPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(UiPuzzleGrid __instance)
        => State.On_UiPuzzleGrid_Start(__instance);

    [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
    [HarmonyPrefix]
    private static void PreStartPuzzle(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).PreStartPuzzle();

    [HarmonyPatch(nameof(UiPuzzleGrid.AttemptGirlFocusSwitch))]
    [HarmonyPrefix]
    public static bool AttemptGirlFocusSwitch(UiPuzzleGrid __instance, ref bool __result)
        => ExpandedUiPuzzleGrid.Get(__instance).AttemptGirlFocusSwitch(ref __result);

    [HarmonyPatch("OnSlotEnter")]
    [HarmonyPostfix]
    public static void OnSlotEnter(UiPuzzleGrid __instance, UiPuzzleSlot slot)
        => ExpandedUiPuzzleGrid.Get(__instance).OnSlotEnter();

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    public static bool Update(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).Update();

    [HarmonyPatch("OnResourceChanged")]
    [HarmonyPostfix]
    public static void OnResourceChanged(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).OnResourceChanged();

    [HarmonyPatch(nameof(UiPuzzleGrid.RefreshGirlDolls))]
    [HarmonyPostfix]
    public static void RefreshGirlDolls(UiPuzzleGrid __instance)
        => ExpandedUiPuzzleGrid.Get(__instance).RefreshGirlDolls();
}

internal class ExpandedUiPuzzleGrid
{
    private static readonly Dictionary<UiPuzzleGrid, ExpandedUiPuzzleGrid> _expansions
        = new Dictionary<UiPuzzleGrid, ExpandedUiPuzzleGrid>();

    public static ExpandedUiPuzzleGrid Get(UiPuzzleGrid uiPuzzleGrid)
    {
        if (!_expansions.TryGetValue(uiPuzzleGrid, out var expansion))
        {
            expansion = new ExpandedUiPuzzleGrid(uiPuzzleGrid);
            _expansions[uiPuzzleGrid] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_status = AccessTools.Field(typeof(UiPuzzleGrid), "_status");
    private static readonly FieldInfo f_state = AccessTools.Field(typeof(UiPuzzleGrid), "_state");
    private static readonly FieldInfo f_moveMatchSet = AccessTools.Field(typeof(UiPuzzleGrid), "_moveMatchSet");
    private static readonly FieldInfo f_warningCheck = AccessTools.Field(typeof(UiPuzzleGrid), "_warningCheck");
    private static readonly FieldInfo f_moveSlotFrom = AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlotFrom");
    private static readonly FieldInfo f_moveSlotTo = AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlotTo");
    private static readonly FieldInfo f_moveSlots = AccessTools.Field(typeof(UiPuzzleGrid), "_moveSlots");
    private static readonly FieldInfo f_moveHasBeenMade = AccessTools.Field(typeof(UiPuzzleGrid), "_moveHasBeenMade");
    private static readonly FieldInfo f_resetTweener = AccessTools.Field(typeof(UiPuzzleGrid), "_resetTweener");
    private static readonly FieldInfo f_isResetting = AccessTools.Field(typeof(UiPuzzleGrid), "_isResetting");

    private static readonly FieldInfo f_stamina = AccessTools.Field(typeof(PuzzleStatusGirl), "_stamina");
    private static readonly FieldInfo f_exhausted = AccessTools.Field(typeof(PuzzleStatusGirl), "_exhausted");
    private static readonly FieldInfo f_upset = AccessTools.Field(typeof(PuzzleStatusGirl), "_upset");

    private static readonly MethodInfo m_warningTooltip = AccessTools.Method(typeof(UiPuzzleGrid), "WarningTooltip");
    private static readonly MethodInfo m_clearMoveSlots = AccessTools.Method(typeof(UiPuzzleGrid), "ClearMoveSlots");
    private static readonly MethodInfo m_consumePuzzleSet = AccessTools.Method(typeof(UiPuzzleGrid), "ConsumePuzzleSet");
    private static readonly MethodInfo m_changeState = AccessTools.Method(typeof(UiPuzzleGrid), "ChangeState");
    private static readonly MethodInfo m_onResetAnimationComplete = AccessTools.Method(typeof(UiPuzzleGrid), "OnResetAnimationComplete");

    public event Action MoveCompleteEvent;

    private readonly UiPuzzleGrid _uiPuzzleGrid;
    private ExpandedUiPuzzleGrid(UiPuzzleGrid uiPuzzleGrid)
    {
        _uiPuzzleGrid = uiPuzzleGrid;
    }

    public bool Update()
    {
        if (!State.IsSingleDate
            || !Game.Session.Location.AtLocationType([LocationType.DATE])
            || !Game.Session.Puzzle.isPuzzleActive
            || Game.Manager.Time.IsPaused(_uiPuzzleGrid.pauseDefinition)
            || !Input.GetMouseButtonUp(0))
        {
            return true;
        }

        var state = f_state.GetValue<PuzzleGameState>(_uiPuzzleGrid);

        if (state != PuzzleGameState.MOVING)
        {
            return true;
        }

        //handle mouse up on a moving state
        //allow any stamina cost for single dates
        //no upset/exhaust
        _uiPuzzleGrid.guideContainer.HideMovementGuides();
        m_warningTooltip.Invoke(_uiPuzzleGrid, [null]);

        var status = f_status.GetValue<PuzzleStatus>(_uiPuzzleGrid);

        var girlStatusFocused = status.girlStatusFocused;
        var girlStatusUnfocused = status.girlStatusUnfocused;

        var moveMatchSet = f_moveMatchSet.GetValue(_uiPuzzleGrid) as PuzzleSet;

        var moveSlotFrom = f_moveSlotFrom.GetValue<UiPuzzleSlot>(_uiPuzzleGrid);
        var moveSlotTo = f_moveSlotTo.GetValue<UiPuzzleSlot>(_uiPuzzleGrid);

        //preform valid move
        if (moveMatchSet != null
            && Game.Session.Puzzle.TutorialStepCheck(moveSlotFrom, moveSlotTo))
        {
            var movesCost = moveMatchSet.GetMovesCost();
            var staminaCostRawFull = moveMatchSet.GetStaminaCost(true, true);

            Game.Session.gameCanvas.effectsContainerLow.dragCursor.Deactivate(null);
            moveMatchSet.DimTokens();

            moveSlotFrom.token.Show();
            moveSlotFrom = null;
            f_moveSlotFrom.SetValue(_uiPuzzleGrid, moveSlotFrom);

            var moveSlots = f_moveSlots.GetValue<List<UiPuzzleSlot>>(_uiPuzzleGrid);

            for (int i = 0; i < moveSlots.Count; i++)
            {
                moveSlots[i].ApplyTempToken();
            }

            var moveModifier = Game.Session.Ailment.Trigger(moveMatchSet);

            f_moveHasBeenMade.SetValue(_uiPuzzleGrid, true);

            var isBigMatch = (bool)m_consumePuzzleSet.Invoke(_uiPuzzleGrid, [moveMatchSet, true]);

            if (!status.bonusRound)
            {
                if (movesCost > 0 && !moveModifier.blockMoveCost)
                {
                    status.AddResourceValue(PuzzleResourceType.MOVES, -movesCost, girlStatusFocused.altGirl);
                }
            }

            Game.Session.Ailment.Trigger(AilmentTriggerType.POST_MOVE, null);

            status.CheckChanges();

            if (girlStatusFocused == status.girlStatusFocused
                && staminaCostRawFull > 1
                && isBigMatch
                && moveMatchSet.HasMatchWithResourceType(PuzzleResourceType.BROKEN, true))
            {
                Game.Session.gameCanvas.GetDoll(girlStatusFocused.altGirl)
                    .ReadDialogTrigger(Game.Session.Puzzle.dtBigMove, DialogLineFormat.UNCHECKED, -1);

                Game.Manager.Audio.Play(AudioCategory.SOUND,
                    _uiPuzzleGrid.sfxsTokenBigMatch[UnityEngine.Random.Range(0, _uiPuzzleGrid.sfxsTokenBigMatch.Length)],
                    _uiPuzzleGrid.pauseDefinition);
            }

            moveMatchSet = null;
            f_moveMatchSet.SetValue(_uiPuzzleGrid, moveMatchSet);

            m_clearMoveSlots.Invoke(_uiPuzzleGrid, []);
        }
        //reset gird for invalid move
        else
        {
            if (moveMatchSet != null)
            {
                moveMatchSet.DimTokens();
            }

            if (moveSlotTo != null && moveSlotTo != moveSlotFrom)
            {
                Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, _uiPuzzleGrid.pauseDefinition);
            }
            else
            {
                Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.DragDrop.sfxDragCancel, null);
            }

            moveMatchSet = null;
            f_moveMatchSet.SetValue(_uiPuzzleGrid, moveMatchSet);

            m_clearMoveSlots.Invoke(_uiPuzzleGrid, []);

            Game.Session.gameCanvas.effectsContainerLow.dragCursor.Deactivate(moveSlotFrom.token.GetTokenSprite(false));

            var resetTweener = Game.Session.gameCanvas.effectsContainerLow.dragCursor.rectTransform
                .DOMove(moveSlotFrom.rectTransform.position, 0.25f, false)
                .SetEase(Ease.OutSine)
                .OnComplete(new TweenCallback(OnResetAnimationComplete));

            f_resetTweener.SetValue(_uiPuzzleGrid, resetTweener);

            Game.Manager.Time.Play(resetTweener, _uiPuzzleGrid.pauseDefinition, 0f);
            f_isResetting.SetValue(_uiPuzzleGrid, true);

            m_changeState.Invoke(_uiPuzzleGrid, [PuzzleGameState.RESETTING]);
        }

        //notify
        MoveCompleteEvent?.Invoke();

        return false;
    }

    public void PreStartPuzzle()
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        //no stamina tokens
        var status = f_status.GetValue<PuzzleStatus>(_uiPuzzleGrid);
        status.girlStatusRight.invalidTokenDefs.Add(Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA));
    }

    public bool AttemptGirlFocusSwitch(ref bool __result)
    {
        if (!State.IsSingleDate)
        {
            return true;
        }

        //don't allow focus switch for single dates
        __result = false;
        return false;
    }

    public void RefreshGirlDolls()
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        Game.Session.gameCanvas.dollRight.SetExhaustion(false, false, true);
        Game.Session.gameCanvas.dollLeft.focusButton.Disable();
    }

    public void OnSlotEnter()
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        var state = f_state.GetValue<PuzzleGameState>(_uiPuzzleGrid);

        if (state != PuzzleGameState.MOVING)
        {
            return;
        }

        //disable warning tooltips
        var moveMatchSet = f_moveMatchSet.GetValue<PuzzleSet>(_uiPuzzleGrid);
        if (moveMatchSet != null)
        {
            var status = f_status.GetValue<PuzzleStatus>(_uiPuzzleGrid);

            if (status.girlStatusFocused.stamina < moveMatchSet.GetStaminaCost(false, false))
            {
                f_warningCheck.SetValue(_uiPuzzleGrid, false);
            }
            else if ((moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.noSpawnMatchTokenDef)
                || moveMatchSet.HasMatchWithTokenDef(status.girlStatusFocused.extraNoSpawnMatchTokenDefs))
                && !status.girlStatusFocused.HasAilment(Game.Session.Puzzle.brokenProtectionAilmentDefinition, true))
            {
                if (!Plugin.ShowSingleUpsetHint.Value)
                {
                    f_warningCheck.SetValue(_uiPuzzleGrid, false);
                }
            }
            else if (status.girlStatusFocused.stamina == moveMatchSet.GetStaminaCost(false, false))
            {
                f_warningCheck.SetValue(_uiPuzzleGrid, false);
            }
        }
    }

    public void OnResourceChanged()
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        var status = f_status.GetValue<PuzzleStatus>(_uiPuzzleGrid);

        //I still want baggage for upset to trigger, so let girl get exhausted/upset 
        //then silently revert it
        //sometimes this still causes girl to be in upset mode which is confusing...
        //set exhaustion must be in a different trigger?
        if (status.girlStatusRight.exhausted || status.girlStatusRight.upset)
        {
            f_stamina.SetValue(status.girlStatusRight, 6);
            f_upset.SetValue(status.girlStatusRight, false);
            f_exhausted.SetValue(status.girlStatusRight, false);
            Game.Session.gameCanvas.dollRight.SetExhaustion(false, false, true);
        }
    }

    private void OnResetAnimationComplete() => m_onResetAnimationComplete.Invoke(_uiPuzzleGrid, []);
}