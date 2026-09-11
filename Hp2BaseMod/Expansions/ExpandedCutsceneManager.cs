using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.ModGameData.Interface;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(CutsceneManager))]
public partial class ExpandedCutsceneManager
{
    [HarmonyPatch(typeof(CutsceneManager))]
    private static class Patch
    {
        [HarmonyPatch("NextStep")]
        [HarmonyPrefix]
        private static bool NextStep(CutsceneManager __instance, bool resetSequence = true)
            => ExpandedCutsceneManager.Get(__instance).NextStep_Prefix(resetSequence);
    }

    private bool NextStep_Prefix(bool resetSequence)
    {
        var branchStepIndices = f_branchStepIndices.GetValue<List<int>>(_core);
        var branches = f_branches.GetValue<List<List<CutsceneStepSubDefinition>>>(_core);

        //don't actually change the index yet in case we don't end up processing
        var currentBranchStepIndex = branchStepIndices[_core.currentBranchIndex] + 1;

        if (currentBranchStepIndex >= branches[_core.currentBranchIndex].Count)
        {
            return true;
        }

        var currentStep = branches[_core.currentBranchIndex][currentBranchStepIndex];
        var stepSequence = f_stepSequence.GetValue<Sequence>(_core);

        if (currentStep is IFunctionalCutsceneStep functional)
        {
            branchStepIndices[_core.currentBranchIndex] = currentBranchStepIndex;
            functional.Complete += On_FunctionStep_Complete;

            if (resetSequence)
            {
                Game.Manager.Time.KillTween(stepSequence, true, true);
                _stepSequence = stepSequence = DOTween.Sequence();
            }

            functional.Act();

            return false;
        }

        if (currentStep.skipStep
            || (currentStep.stepType.ToString().ToUpper().Contains("PUZZLE")
                && !Game.Session.Location.AtLocationType(LocationType.DATE)))
        {
            return true;
        }

        if (!WillDollBeRandomized(currentStep))
        {
            return true;
        }

        var args = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(args);
        var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

        //we're committed, no more return trues, apply changes to the actual branchStepIndices and use that from now on
        //and set the other values

        if (resetSequence)
        {
            Game.Manager.Time.KillTween(stepSequence, true, true);
            _stepSequence = stepSequence = DOTween.Sequence();
        }

        branchStepIndices[_core.currentBranchIndex] = currentBranchStepIndex;
        _audioLink = null;
        _currentStep = currentStep;

        HandleStepType(currentStep,
            branches,
            branchStepIndices,
            uiDoll,
            stepSequence);

        return false;
    }

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
    public void HandleStepType(CutsceneStepSubDefinition currentStep,
        List<List<CutsceneStepSubDefinition>> branches,
        List<int> branchStepIndices,
        UiDoll uiDoll,
        Sequence stepSequence)
    {
        var _cutsceneManager = Game.Session.Cutscenes;

        var isBannerTextNull = false;
        var windowShown = false;

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
                _specialStep = UnityEngine.Object.Instantiate(currentStep.specialStepPrefab);
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
                        _bannerText = bannerText;

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
                        isBannerTextNull = true;
                    }
                }
                else if (bannerText != null)
                {
                    bannerText.Hide(stepSequence, 0f);
                }
                else
                {
                    isBannerTextNull = true;
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
                    windowShown = true;
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
                _audioLink = Game.Manager.Audio.Play(
                    (!currentStep.boolValue) 
                        ? AudioCategory.SOUND 
                        : AudioCategory.VOICE, 
                    currentStep.audioKlip, 
                    null);
                break;
            case CutsceneStepType.CLEAR_MOOD:
                uiDoll.ClearMood();
                break;
            case CutsceneStepType.PARTICLE_EMITTER:
                var emitterBehavior = UnityEngine.Object.Instantiate(currentStep.emitterBehavior);
                _emitterBehavior = emitterBehavior;

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
                    _targetDoll = targetDoll;

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

        if (isBannerTextNull)
        {
            NextStep(true);
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
                        NextStep(true);
                        break;
                    case CutsceneStepType.SPECIAL_STEP:
                        f_specialStep.GetValue<CutsceneStepSpecial>(_cutsceneManager).StepCompleteEvent += OnSpecialStepComplete_Hook;
                        break;
                    case CutsceneStepType.DIALOG_LINE:
                    case CutsceneStepType.DIALOG_TRIGGER:
                        if (!currentStep.proceedBool)
                        {
                            uiDoll.DialogLineCompleteEvent += OnDialogLineComplete_Hook;
                        }
                        else
                        {
                            uiDoll.DialogBoxHiddenEvent += OnDialogBoxHidden_Hook;
                        }

                        break;
                    case CutsceneStepType.DOUBLE_TRIGGER:
                        Game.Session.Dialog.DialogQueueEmptyEvent += OnDialogQueueEmpty_Hook;
                        break;
                    case CutsceneStepType.DIALOG_OPTIONS:
                        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected_Hook;
                        break;
                    case CutsceneStepType.DOLL_MOVE:
                    case CutsceneStepType.TOGGLE_PHONE:
                    case CutsceneStepType.PUZZLE_GRID:
                    case CutsceneStepType.BANNER_TEXT:
                    case CutsceneStepType.PLAY_ANIMATION:
                    case CutsceneStepType.TOGGLE_OVERLAY:
                        stepSequence.OnComplete(new TweenCallback(OnStepSequenceComplete_Hook));
                        break;
                    case CutsceneStepType.PUZZLE_REFOCUS:
                    case CutsceneStepType.SOUND_EFFECT:
                    case CutsceneStepType.PARTICLE_EMITTER:
                    case CutsceneStepType.SHOW_NOTIFICATION:
                        _checkStepProceed = true;
                        break;
                    case CutsceneStepType.SHOW_WINDOW:
                        if (!currentStep.boolValue)
                        {
                            if (!currentStep.proceedBool)
                            {
                                Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden_Hook;
                            }
                            else
                            {
                                Game.Manager.Windows.WindowQueueCompleteEvent += OnWindowQueueComplete_Hook;
                            }
                        }
                        else
                        {
                            if (windowShown)
                            {
                                Game.Manager.Windows.WindowShownEvent += OnWindowShown_Hook;
                            }
                            else
                            {
                                Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden_Hook;
                            }
                        }
                        break;
                    case CutsceneStepType.USE_CELLPHONE:
                        Game.Session.gameCanvas.cellphone.ClosedEvent += OnCellphoneClosed_Hook;
                        break;
                }
                break;
            case CutsceneStepProceedType.INSTANT:
                NextStep(stepSequence.Duration(true) <= 0f);
                break;
            case CutsceneStepProceedType.WAIT:
                _isWaiting = true;
                _waitDuration = currentStep.proceedFloat;
                _waitTimestamp = Game.Manager.Time.Lifetime(_cutsceneManager.pauseDefinition);
                break;
            case CutsceneStepProceedType.STANDBY:
                _isOnStandby = true;
                _standbyProceed = false;
                break;
        }
    }

    /// <summary>
    /// If the step will cause the doll selected for the step to be randomized
    /// </summary>
    /// <param name="currentStep"></param>
    /// <returns></returns>
    public bool WillDollBeRandomized(CutsceneStepSubDefinition currentStep)
        => WillDollBeRandomized(currentStep.dollTargetType, currentStep.targetGirlDefinition, currentStep.targetDollOrientation, currentStep.targetAlt);

    /// <summary>
    /// If the step properties will cause the doll selected for the step to be randomized
    /// </summary>
    /// <param name="dollTargetType"></param>
    /// <param name="targetGirlDef"></param>
    /// <param name="targetDollOrientation"></param>
    /// <param name="targetAlt"></param>
    /// <returns></returns>
    public bool WillDollBeRandomized(CutsceneStepDollTargetType dollTargetType, GirlDefinition targetGirlDef, DollOrientationType targetDollOrientation, bool targetAlt)
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

    private void OnSpecialStepComplete_Hook(CutsceneStepSpecial specialStep)
    {
        specialStep.StepCompleteEvent -= OnSpecialStepComplete_Hook;
        NextStep(true);
    }

    private void OnDialogLineComplete_Hook(UiDoll doll)
    {
        doll.DialogLineCompleteEvent -= OnDialogLineComplete_Hook;
        NextStep(true);
    }

    private void OnDialogBoxHidden_Hook(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnDialogBoxHidden_Hook;
        NextStep(true);
    }

    private void OnDialogQueueEmpty_Hook()
    {
        Game.Session.Dialog.DialogQueueEmptyEvent -= OnDialogQueueEmpty_Hook;
        NextStep(true);
    }

    private void OnCellphoneClosed_Hook()
    {
        Game.Session.gameCanvas.cellphone.ClosedEvent -= OnCellphoneClosed_Hook;
        NextStep(true);
    }

    private void OnWindowShown_Hook()
    {
        Game.Manager.Windows.WindowShownEvent -= OnWindowShown_Hook;
        NextStep(true);
    }

    private void OnWindowHidden_Hook()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden_Hook;
        NextStep(true);
    }

    private void OnWindowQueueComplete_Hook()
    {
        Game.Manager.Windows.WindowQueueCompleteEvent -= OnWindowQueueComplete_Hook;
        NextStep(true);
    }

    private void OnStepSequenceComplete_Hook()
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
                    _bannerText = null;
                }
                break;
            case CutsceneStepType.PLAY_ANIMATION:
                if (currentStep.animationType == CutsceneStepAnimationType.SCREEN_SHAKE)
                {
                    Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
                }
                break;
        }
        NextStep(true);
    }

    private void OnDialogOptionSelected_Hook()
    {
        var currentStep = f_currentStep.GetValue<CutsceneStepSubDefinition>(Game.Session.Cutscenes);
        var branches = f_branches.GetValue<List<List<CutsceneStepSubDefinition>>>(Game.Session.Cutscenes);
        var branchStepIndices = f_branchStepIndices.GetValue<List<int>>(Game.Session.Cutscenes);

        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected_Hook;
        CutsceneStepType stepType = currentStep.stepType;
        if (stepType == CutsceneStepType.DIALOG_OPTIONS)
        {
            branches.Add(currentStep.dialogOptions[Game.Session.Dialog.selectedDialogOptionIndex].steps);
            branchStepIndices.Add(-1);
        }
        NextStep(true);
    }

    private void On_FunctionStep_Complete(IFunctionalCutsceneStep sender)
    {
        sender.Complete -= On_FunctionStep_Complete;
        NextStep(false);
    }
}
