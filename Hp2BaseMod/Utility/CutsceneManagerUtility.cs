using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static class CutsceneManagerUtility
{
    private static FieldInfo f_specialStep = AccessTools.Field(typeof(CutsceneManager), "_specialStep");
    private static FieldInfo f_bannerText = AccessTools.Field(typeof(CutsceneManager), "_bannerText");
    private static FieldInfo f_innerCutsceneDefinition = AccessTools.Field(typeof(CutsceneManager), "_innerCutsceneDefinition");
    private static FieldInfo f_emitterBehavior = AccessTools.Field(typeof(CutsceneManager), "_emitterBehavior");
    private static FieldInfo f_targetDoll = AccessTools.Field(typeof(CutsceneManager), "_targetDoll");
    private static FieldInfo f_isWaiting = AccessTools.Field(typeof(CutsceneManager), "_isWaiting");
    private static FieldInfo f_waitDuration = AccessTools.Field(typeof(CutsceneManager), "_waitDuration");
    private static FieldInfo f_waitTimestamp = AccessTools.Field(typeof(CutsceneManager), "_waitTimestamp");
    private static FieldInfo f_isOnStandby = AccessTools.Field(typeof(CutsceneManager), "_isOnStandby");
    private static FieldInfo f_standbyProceed = AccessTools.Field(typeof(CutsceneManager), "_standbyProceed");
    private static FieldInfo f_checkStepProceed = AccessTools.Field(typeof(CutsceneManager), "_checkStepProceed");
    private static FieldInfo f_audioLink = AccessTools.Field(typeof(CutsceneManager), "_audioLink");
    private static FieldInfo f_currentStep = AccessTools.Field(typeof(CutsceneManager), "_currentStep");
    private static FieldInfo f_branchStepIndices = AccessTools.Field(typeof(CutsceneManager), "_branchStepIndices");
    private static FieldInfo f_branches = AccessTools.Field(typeof(CutsceneManager), "_branches");
    private static MethodInfo m_nextStep = AccessTools.Method(typeof(CutsceneManager), "NextStep");

    /// <summary>
    /// Handles the function of CutsceneManager.NextStep() 
    /// after _branchStepIndices[this.currentBranchIndex] >= _branches[this.currentBranchIndex].Count is false
    /// after uiDoll is selected.
    /// 
    /// If using this make sure the branch step indices are updated properly
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="branches"></param>
    /// <param name="branchStepIndices"></param>
    /// <param name="uiDoll"></param>
    /// <param name="stepSequence"></param>
    public static void HandleStepType(CutsceneStepSubDefinition currentStep,
        List<List<CutsceneStepSubDefinition>> branches,
        List<int> branchStepIndices,
        UiDoll uiDoll,
        Sequence stepSequence)
    {
        var _cutsceneManager = Game.Session.Cutscenes;

        bool flag = false;//isBannerTextNull
        bool flag2 = false;//window

        switch (currentStep.stepType)
        {
            case CutsceneStepType.BRANCH:
                {
                    foreach (var branch in currentStep.branches)
                    {
                        if (Game.Session.Logic.IsConditionListMet(branch.conditions))
                        {
                            if (branch.cutsceneDefinition != null)
                            {
                                branches.Add(branch.cutsceneDefinition.steps);
                            }
                            else
                            {
                                branches.Add(branch.steps);
                            }
                            branchStepIndices.Add(-1);
                            break;
                        }
                    }
                    break;
                }
            case CutsceneStepType.GAME_ACTION:
                Game.Session.Logic.PerformAction(currentStep.logicAction);
                break;
            case CutsceneStepType.SPECIAL_STEP:
                f_specialStep.SetValue(_cutsceneManager, UnityEngine.Object.Instantiate(currentStep.specialStepPrefab));
                break;
            case CutsceneStepType.CHANGE_EXPRESSION:
                if (!currentStep.setMood)
                {
                    uiDoll.ChangeExpression(currentStep.expressionType, currentStep.boolValue);
                }
                else
                {
                    uiDoll.SetMood(currentStep.expressionType, currentStep.floatValue);
                }
                break;
            case CutsceneStepType.DIALOG_LINE:
                uiDoll.ReadDialogLine(currentStep.dialogLine,
                    currentStep.proceedType == CutsceneStepProceedType.AUTOMATIC
                        ? DialogLineFormat.ACTIVE
                        : DialogLineFormat.UNCHECKED,
                    -1);
                uiDoll.isDialogBoxLocked = currentStep.boolValue;
                break;
            case CutsceneStepType.DIALOG_TRIGGER:
                uiDoll.ReadDialogTrigger(currentStep.dialogTriggerDefinition,
                    currentStep.proceedType == CutsceneStepProceedType.AUTOMATIC
                        ? DialogLineFormat.ACTIVE
                        : DialogLineFormat.UNCHECKED,
                    -1);
                break;
            case CutsceneStepType.DOUBLE_TRIGGER:
                {
                    bool flag3 = Game.Session.Location.AtLocationType([LocationType.DATE]) && currentStep.boolValue
                        ? Game.Session.Puzzle.puzzleStatus.altGirlFocused
                        : MathUtils.RandomBool();

                    Game.Session.Dialog.QueueDialog(flag3, currentStep.dialogTriggerDefinition, DialogLineFormat.ACTIVE);
                    Game.Session.Dialog.QueueDialog(!flag3, currentStep.dialogTriggerDefinition.responseTrigger, DialogLineFormat.ACTIVE);
                    Game.Session.Dialog.ProcessDialogQueue(currentStep.proceedBool);
                    break;
                }
            case CutsceneStepType.DIALOG_OPTIONS:
                {
                    List<string> list = new List<string>();
                    foreach (var option in currentStep.dialogOptions)
                    {
                        list.Add((option.yuri && Game.Persistence.playerFile.settingGender == SettingGender.FEMALE)
                                                    ? option.yuriDialogOptionText
                                                    : option.dialogOptionText);
                    }
                    Game.Session.Dialog.ShowDialogOptions(list, currentStep.boolValue, true);
                    break;
                }
            case CutsceneStepType.DOLL_MOVE:
                {
                    uiDoll.DetermineCurrentPositionType();

                    Ease ease = Ease.InOutCubic;
                    if (currentStep.easeType != Ease.Unset)
                    {
                        ease = currentStep.easeType;
                    }

                    stepSequence.Insert(0f,
                        uiDoll.slideLayer.DOAnchorPos(uiDoll.GetPositionByType(currentStep.dollPositionType),
                            currentStep.floatValue > 0f
                                ? currentStep.floatValue
                                : 1f, false).SetEase(ease));
                    break;
                }
            case CutsceneStepType.LOAD_GIRL:
                if (currentStep.girlDefinition != null)
                {
                    if (currentStep.boolValue)
                    {
                        uiDoll.LoadGirl(currentStep.girlDefinition, currentStep.expressionIndex, currentStep.hairstyleIndex, currentStep.outfitIndex, null);
                    }
                    else
                    {
                        uiDoll.LoadGirl(currentStep.girlDefinition, -1, -1, -1, null);
                    }
                }
                else
                {
                    uiDoll.UnloadGirl();
                }
                break;
            case CutsceneStepType.TOGGLE_PHONE:
                if (currentStep.intValue >= 0)
                {
                    stepSequence.Insert(0f,
                        Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(currentStep.boolValue
                                ? Game.Session.gameCanvas.header.yValues.y
                                : Game.Session.gameCanvas.header.yValues.x,
                            0.5f,
                            false)
                        .SetEase(currentStep.boolValue
                            ? Ease.OutCubic
                            : Ease.InCubic));
                }
                if (currentStep.intValue <= 0)
                {
                    stepSequence.Insert(0f,
                        Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(currentStep.boolValue
                                ? Game.Session.gameCanvas.cellphone.yValues.y
                                : Game.Session.gameCanvas.cellphone.yValues.x,
                            0.5f,
                            false)
                        .SetEase(currentStep.boolValue
                            ? Ease.OutCubic
                            : Ease.InCubic));
                }
                break;
            case CutsceneStepType.REWIND:
                {
                    for (int k = Mathf.Abs(currentStep.intValue) + 1; k > 0; k--)
                    {
                        if (branches.Count <= 1 && branchStepIndices[_cutsceneManager.currentBranchIndex] < 0)
                        {
                            break;
                        }
                        List<int> branchStepIndices2 = branchStepIndices;

                        var num2 = _cutsceneManager.currentBranchIndex;
                        var num = branchStepIndices2[num2];
                        branchStepIndices2[num2] = num - 1;

                        if (branchStepIndices[_cutsceneManager.currentBranchIndex] < -1)
                        {
                            branchStepIndices.RemoveAt(_cutsceneManager.currentBranchIndex);
                            branches.RemoveAt(_cutsceneManager.currentBranchIndex);
                            List<int> branchStepIndices3 = branchStepIndices;

                            num = _cutsceneManager.currentBranchIndex;
                            num2 = branchStepIndices3[num];
                            branchStepIndices3[num] = num2 - 1;
                        }
                    }
                    break;
                }
            case CutsceneStepType.PUZZLE_GRID:
                if (currentStep.boolValue)
                {
                    Game.Session.Puzzle.puzzleGrid.backgroundBlur.Enable();
                }

                stepSequence.Insert(0f, Game.Session.Puzzle.puzzleGrid.rectTransform.DOScale(Vector3.one * (currentStep.boolValue ? 1f : 0.8f), 0.5f).SetEase(currentStep.boolValue ? Ease.OutBack : Ease.InBack));
                stepSequence.Insert(0f, Game.Session.Puzzle.puzzleGrid.canvasGroup.DOFade(currentStep.boolValue ? 1 : 0, 0.5f).SetEase(Ease.Linear));
                stepSequence.Insert(currentStep.boolValue ? 0.5f : 0f, Game.Session.Puzzle.puzzleGrid.dateGiftsContainer.canvasGroup.DOFade(currentStep.boolValue ? 1 : 0, 0f).SetEase(Ease.Linear));
                break;
            case CutsceneStepType.BANNER_TEXT:
                var bannerText = f_bannerText.GetValue<BannerTextBehavior>(_cutsceneManager);

                if (currentStep.boolValue)
                {
                    if (bannerText == null)
                    {
                        bannerText = UnityEngine.Object.Instantiate(currentStep.bannerTextPrefab);
                        f_bannerText.SetValue(_cutsceneManager, bannerText);

                        bannerText.Init(stepSequence, currentStep.intValue);
                        if (!bannerText.autoHide)
                        {
                            stepSequence.PrependInterval(1f);
                            stepSequence.AppendInterval(1f);
                        }
                        else
                        {
                            stepSequence.PrependInterval(bannerText.showDelay);
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else if (bannerText != null)
                {
                    bannerText.Hide(stepSequence, 0f);
                }
                else
                {
                    flag = true;
                }
                break;
            case CutsceneStepType.PUZZLE_REFOCUS:
                Game.Session.Puzzle.puzzleGrid.RefreshGirlDolls();
                if (currentStep.boolValue)
                {
                    Game.Session.gameCanvas.dollLeft.ClearMood();
                    Game.Session.gameCanvas.dollRight.ClearMood();
                    Game.Session.gameCanvas.dollLeft.ChangeExpression(Game.Session.gameCanvas.dollLeft.soulGirlDefinition.failureExpressionIndex, false);
                    Game.Session.gameCanvas.dollRight.ChangeExpression(Game.Session.gameCanvas.dollRight.soulGirlDefinition.failureExpressionIndex, false);
                }
                break;
            case CutsceneStepType.SET_EXHAUSTION:
                uiDoll.SetExhaustion(currentStep.boolValue, false, true);
                break;
            case CutsceneStepType.PLAY_ANIMATION:
                if (currentStep.animationType == CutsceneStepAnimationType.SCREEN_SHAKE)
                {
                    stepSequence.Insert(0f, Game.Session.gameCanvas.bgLocations.rectTransform.DOShakeAnchorPos(currentStep.floatValue, currentStep.intValue, 20, 90f, false, currentStep.boolValue));
                }
                break;
            case CutsceneStepType.SUB_CUTSCENE:
                {
                    GirlPairDefinition currentGirlPair = Game.Session.Location.currentGirlPair;
                    CutsceneDefinition cutsceneDefinition = null;
                    switch (currentStep.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            cutsceneDefinition = currentStep.subCutsceneDefinition;
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            if (currentGirlPair != null)
                            {
                                cutsceneDefinition = currentGirlPair.relationshipCutsceneDefinitions[Mathf.Clamp((int)currentStep.girlPairRelationshipType, 0, currentGirlPair.relationshipCutsceneDefinitions.Count - 1)];
                            }
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND:
                            if (currentGirlPair != null)
                            {
                                cutsceneDefinition = currentGirlPair.relationshipCutsceneDefinitions[Mathf.Clamp(Game.Session.Puzzle.puzzleStatus.roundIndex, 0, currentGirlPair.relationshipCutsceneDefinitions.Count - 1)];
                            }
                            break;
                        case CutsceneStepSubCutsceneType.INNER:
                            cutsceneDefinition = f_innerCutsceneDefinition.GetValue<CutsceneDefinition>(_cutsceneManager);
                            break;
                    }
                    if (cutsceneDefinition != null && cutsceneDefinition.steps.Count > 0)
                    {
                        branches.Add(cutsceneDefinition.steps);
                        branchStepIndices.Add(-1);
                    }
                    break;
                }
            case CutsceneStepType.SHOW_WINDOW:
                if (!currentStep.boolValue)
                {
                    Game.Manager.Windows.ShowWindow(currentStep.windowPrefab, true);
                }
                else if (!Game.Manager.Windows.IsWindowActive(null, true, true))
                {
                    Game.Manager.Windows.ShowWindow(currentStep.windowPrefab, false);
                    flag2 = true;
                }
                else
                {
                    Game.Manager.Windows.HideWindow();
                }
                break;
            case CutsceneStepType.USE_CELLPHONE:
                Game.Session.gameCanvas.cellphone.openAppIndex = currentStep.intValue;
                Game.Session.gameCanvas.cellphone.openAppLocked = currentStep.boolValue;
                if (!StringUtils.IsEmpty(currentStep.stringValue))
                {
                    Game.Session.gameCanvas.cellphone.FreezeAppButtons(true, StringUtils.ParseIntValue(currentStep.stringValue.Split([',']), false));
                }
                Game.Session.gameCanvas.cellphone.Open();
                break;
            case CutsceneStepType.SHAKE_SCREEN:
                Game.Session.gameCanvas.bgLocations.ShakeScreen(currentStep.floatValue, currentStep.intValue, currentStep.boolValue);
                break;
            case CutsceneStepType.RESET_DOLLS:
                Game.Session.Location.ResetDolls(false);
                break;
            case CutsceneStepType.TOGGLE_OVERLAY:
                stepSequence.Insert(0f, Game.Session.gameCanvas.overlayCanvasGroup.DOFade(currentStep.boolValue ? 1 : 0, currentStep.floatValue).SetEase(Ease.Linear));
                break;
            case CutsceneStepType.SOUND_EFFECT:
                f_audioLink.SetValue(_cutsceneManager, Game.Manager.Audio.Play((!currentStep.boolValue) ? AudioCategory.SOUND : AudioCategory.VOICE, currentStep.audioKlip, null));
                break;
            case CutsceneStepType.CLEAR_MOOD:
                uiDoll.ClearMood();
                break;
            case CutsceneStepType.PARTICLE_EMITTER:
                var emitterBehavior = UnityEngine.Object.Instantiate<EmitterBehavior>(currentStep.emitterBehavior);
                f_emitterBehavior.SetValue(_cutsceneManager, emitterBehavior);

                emitterBehavior.transform.SetParent(Game.Manager.Ui.GetEffectsContainer(currentStep.intValue).particleContainer, false);
                emitterBehavior.transform.position = Game.Manager.gameCamera.ScreenScale(currentStep.position);
                if (!emitterBehavior.autoInit)
                {
                    emitterBehavior.Init(false);
                }
                break;
            case CutsceneStepType.SHOW_NOTIFICATION:
                {
                    var targetDoll = uiDoll;
                    f_targetDoll.SetValue(_cutsceneManager, targetDoll);

                    var text = currentStep.stringValue;
                    var notificationType = currentStep.notificationType;
                    if (notificationType == CutsceneStepNotificationType.BAGGAGE)
                    {
                        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(targetDoll.girlDefinition);
                        text = _cutsceneManager.notificationMessages[1].Replace("(GIRL)", playerFileGirl.girlDefinition.girlName).Replace("(BAGGAGE)", playerFileGirl.girlDefinition.baggageItemDefs[playerFileGirl.learnedBaggage[playerFileGirl.learnedBaggage.Count - 1]].itemName);
                    }
                    targetDoll.notificationBox.Show(text, currentStep.floatValue, false);
                    break;
                }
        }

        if (flag)
        {
            m_nextStep.Invoke(_cutsceneManager, [true]);
            return;
        }

        if (stepSequence.Duration(true) > 0f && currentStep.proceedType != CutsceneStepProceedType.INSTANT)
        {
            Game.Manager.Time.Play(stepSequence, _cutsceneManager.pauseDefinition, 0f);
        }

        switch (currentStep.proceedType)
        {
            case CutsceneStepProceedType.AUTOMATIC:
                switch (currentStep.stepType)
                {
                    case CutsceneStepType.NOTHING:
                    case CutsceneStepType.BRANCH:
                    case CutsceneStepType.GAME_ACTION:
                    case CutsceneStepType.CHANGE_EXPRESSION:
                    case CutsceneStepType.LOAD_GIRL:
                    case CutsceneStepType.REWIND:
                    case CutsceneStepType.SET_EXHAUSTION:
                    case CutsceneStepType.SUB_CUTSCENE:
                    case CutsceneStepType.SHAKE_SCREEN:
                    case CutsceneStepType.RESET_DOLLS:
                    case CutsceneStepType.CLEAR_MOOD:
                        m_nextStep.Invoke(_cutsceneManager, [true]);
                        break;
                    case CutsceneStepType.SPECIAL_STEP:
                        f_specialStep.GetValue<CutsceneStepSpecial>(_cutsceneManager).StepCompleteEvent += OnSpecialStepComplete;
                        break;
                    case CutsceneStepType.DIALOG_LINE:
                    case CutsceneStepType.DIALOG_TRIGGER:
                        if (!currentStep.proceedBool)
                        {
                            uiDoll.DialogLineCompleteEvent += OnDialogLineComplete;
                        }
                        else
                        {
                            uiDoll.DialogBoxHiddenEvent += OnDialogBoxHidden;
                        }

                        break;
                    case CutsceneStepType.DOUBLE_TRIGGER:
                        Game.Session.Dialog.DialogQueueEmptyEvent += OnDialogQueueEmpty;
                        break;
                    case CutsceneStepType.DIALOG_OPTIONS:
                        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected;
                        break;
                    case CutsceneStepType.DOLL_MOVE:
                    case CutsceneStepType.TOGGLE_PHONE:
                    case CutsceneStepType.PUZZLE_GRID:
                    case CutsceneStepType.BANNER_TEXT:
                    case CutsceneStepType.PLAY_ANIMATION:
                    case CutsceneStepType.TOGGLE_OVERLAY:
                        stepSequence.OnComplete(new TweenCallback(OnStepSequenceComplete));
                        break;
                    case CutsceneStepType.PUZZLE_REFOCUS:
                    case CutsceneStepType.SOUND_EFFECT:
                    case CutsceneStepType.PARTICLE_EMITTER:
                    case CutsceneStepType.SHOW_NOTIFICATION:
                        f_checkStepProceed.SetValue(_cutsceneManager, true);
                        break;
                    case CutsceneStepType.SHOW_WINDOW:
                        if (!currentStep.boolValue)
                        {
                            if (!currentStep.proceedBool)
                            {
                                Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                            }
                            else
                            {
                                Game.Manager.Windows.WindowQueueCompleteEvent += OnWindowQueueComplete;
                            }
                        }
                        else
                        {
                            if (flag2)
                            {
                                Game.Manager.Windows.WindowShownEvent += OnWindowShown;
                            }
                            else
                            {
                                Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                            }
                        }
                        break;
                    case CutsceneStepType.USE_CELLPHONE:
                        Game.Session.gameCanvas.cellphone.ClosedEvent += OnCellphoneClosed;
                        break;
                }
                break;
            case CutsceneStepProceedType.INSTANT:
                m_nextStep.Invoke(_cutsceneManager, [stepSequence.Duration(true) <= 0f]);
                break;
            case CutsceneStepProceedType.WAIT:
                f_isWaiting.SetValue(_cutsceneManager, true);
                f_waitDuration.SetValue(_cutsceneManager, currentStep.proceedFloat);
                f_waitTimestamp.SetValue(_cutsceneManager, Game.Manager.Time.Lifetime(_cutsceneManager.pauseDefinition));
                break;
            case CutsceneStepProceedType.STANDBY:
                f_isOnStandby.SetValue(_cutsceneManager, true);
                f_standbyProceed.SetValue(_cutsceneManager, false);
                break;
        }
    }

    /// <summary>
    /// If the step will cause the doll selected for the step to be randomized
    /// </summary>
    /// <param name="currentStep"></param>
    /// <returns></returns>
    public static bool WillDollBeRandomized(CutsceneStepSubDefinition currentStep)
        => WillDollBeRandomized(currentStep.dollTargetType, currentStep.targetGirlDefinition, currentStep.targetDollOrientation, currentStep.targetAlt);

    /// <summary>
    /// If the step properties will cause the doll selected for the step to be randomized
    /// </summary>
    /// <param name="dollTargetType"></param>
    /// <param name="targetGirlDef"></param>
    /// <param name="targetDollOrientation"></param>
    /// <param name="targetAlt"></param>
    /// <returns></returns>
    public static bool WillDollBeRandomized(CutsceneStepDollTargetType dollTargetType, GirlDefinition targetGirlDef, DollOrientationType targetDollOrientation, bool targetAlt)
    {
        UiDoll uiDoll = null;
        switch (dollTargetType)
        {
            case CutsceneStepDollTargetType.GIRL_DEFINITION:
                uiDoll = Game.Session.gameCanvas.GetDoll(targetGirlDef);
                if (targetAlt && uiDoll != null && uiDoll.orientation != DollOrientationType.MIDDLE)
                {
                    uiDoll = Game.Session.gameCanvas.GetDoll(uiDoll.mirrored);
                }
                break;
            case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                uiDoll = Game.Session.gameCanvas.GetDoll(targetDollOrientation);
                break;
            case CutsceneStepDollTargetType.RANDOM:
                return true;
            case CutsceneStepDollTargetType.FOCUSED:
                if (!targetAlt)
                {
                    uiDoll = Game.Session.gameCanvas.GetDoll(Game.Session.Puzzle.puzzleStatus.altGirlFocused);
                }
                else
                {
                    uiDoll = Game.Session.gameCanvas.GetDoll(!Game.Session.Puzzle.puzzleStatus.altGirlFocused);
                }
                break;
        }

        return uiDoll == null;
    }

    private static void OnSpecialStepComplete(CutsceneStepSpecial specialStep)
    {
        specialStep.StepCompleteEvent -= OnSpecialStepComplete;

        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnDialogLineComplete(UiDoll doll)
    {
        doll.DialogLineCompleteEvent -= OnDialogLineComplete;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnDialogBoxHidden(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnDialogBoxHidden;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnDialogQueueEmpty()
    {
        Game.Session.Dialog.DialogQueueEmptyEvent -= OnDialogQueueEmpty;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnCellphoneClosed()
    {
        Game.Session.gameCanvas.cellphone.ClosedEvent -= OnCellphoneClosed;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnWindowShown()
    {
        Game.Manager.Windows.WindowShownEvent -= OnWindowShown;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnWindowHidden()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnWindowQueueComplete()
    {
        Game.Manager.Windows.WindowQueueCompleteEvent -= OnWindowQueueComplete;
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnStepSequenceComplete()
    {
        //no unsub because it's used in a tween callback instead of an event
        var currentStep = f_currentStep.GetValue<CutsceneStepSubDefinition>(Game.Session.Cutscenes);
        var bannerText = f_bannerText.GetValue<BannerTextBehavior>(Game.Session.Cutscenes);

        switch (currentStep.stepType)
        {
            case CutsceneStepType.PUZZLE_GRID:
                if (!currentStep.boolValue)
                {
                    Game.Session.Puzzle.puzzleGrid.backgroundBlur.Disable();
                }
                break;
            case CutsceneStepType.BANNER_TEXT:
                if (!currentStep.boolValue || bannerText.autoHide)
                {
                    UnityEngine.Object.Destroy(bannerText.gameObject);
                    f_bannerText.SetValue(Game.Session.Cutscenes, null);
                }
                break;
            case CutsceneStepType.PLAY_ANIMATION:
                if (currentStep.animationType == CutsceneStepAnimationType.SCREEN_SHAKE)
                {
                    Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
                }
                break;
        }
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }

    private static void OnDialogOptionSelected()
    {
        var currentStep = f_currentStep.GetValue<CutsceneStepSubDefinition>(Game.Session.Cutscenes);
        var branches = f_branches.GetValue<List<List<CutsceneStepSubDefinition>>>(Game.Session.Cutscenes);
        var branchStepIndices = f_branchStepIndices.GetValue<List<int>>(Game.Session.Cutscenes);

        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected;
        CutsceneStepType stepType = currentStep.stepType;
        if (stepType == CutsceneStepType.DIALOG_OPTIONS)
        {
            branches.Add(currentStep.dialogOptions[Game.Session.Dialog.selectedDialogOptionIndex].steps);
            branchStepIndices.Add(-1);
        }
        m_nextStep.Invoke(Game.Session.Cutscenes, [true]);
    }
}