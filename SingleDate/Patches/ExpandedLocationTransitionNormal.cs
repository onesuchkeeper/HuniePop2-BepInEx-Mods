using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using SingleDate;
using UnityEngine;

[HarmonyPatch(typeof(LocationTransitionNormal))]
internal static class LocationTransitionNormalPatch
{
    [HarmonyPatch("ArriveStep")]
    [HarmonyPrefix]
    public static bool ArriveStep(LocationTransitionNormal __instance)
        => ExpandedLocationTransitionNormal.Get(__instance).ArriveStep();

    [HarmonyPatch("DepartStep")]
    [HarmonyPrefix]
    public static bool DepartStep(LocationTransitionNormal __instance)
        => ExpandedLocationTransitionNormal.Get(__instance).DepartStep();
}

internal class ExpandedLocationTransitionNormal
{
    private static Dictionary<LocationTransitionNormal, ExpandedLocationTransitionNormal> _extendedTransitions
        = new Dictionary<LocationTransitionNormal, ExpandedLocationTransitionNormal>();

    public static ExpandedLocationTransitionNormal Get(LocationTransitionNormal locationTransitionNormal)
    {
        if (!_extendedTransitions.TryGetValue(locationTransitionNormal, out var extended))
        {
            extended = new ExpandedLocationTransitionNormal(locationTransitionNormal);
            _extendedTransitions[locationTransitionNormal] = extended;
        }

        return extended;
    }

    private static readonly FieldInfo _stepIndex = AccessTools.Field(typeof(LocationTransitionNormal), "_stepIndex");
    private static readonly FieldInfo _gameSaved = AccessTools.Field(typeof(LocationTransitionNormal), "_gameSaved");
    private static readonly FieldInfo _arriveWithGirls = AccessTools.Field(typeof(LocationTransitionNormal), "_arriveWithGirls");
    private static readonly FieldInfo _initialArrive = AccessTools.Field(typeof(LocationTransitionNormal), "_initialArrive");
    private static readonly FieldInfo _sequence = AccessTools.Field(typeof(LocationTransitionNormal), "_sequence");
    private static readonly MethodInfo m_departStep = AccessTools.Method(typeof(LocationTransitionNormal), "DepartStep");
    private static readonly MethodInfo m_arrivalComplete = AccessTools.Method(typeof(LocationTransitionNormal), "ArrivalComplete");
    private static readonly MethodInfo m_onArriveAnimationsComplete = AccessTools.Method(typeof(LocationTransitionNormal), "OnArriveAnimationsComplete");

    private static readonly FieldInfo _altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    private bool _singleDropZone = false;

    private readonly LocationTransitionNormal _core;
    public ExpandedLocationTransitionNormal(LocationTransitionNormal core)
    {
        _core = core;
    }

    public bool ArriveStep()
    {
        var stepIndex = _stepIndex.GetValue<int>(_core);
        stepIndex++;
        _stepIndex.SetValue(_core, stepIndex);

        if (stepIndex != 0)
        {
            if (stepIndex != 1)
            {
                return false;
            }

            m_arrivalComplete.Invoke(_core, null);
            return false;
        }
        else
        {
            ModInterface.Log.LogInfo();
            if (State.IsSingleDate)
            {
                var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;

                Game.Session.Puzzle.puzzleGrid.transform.position = new Vector3(State.DefaultPuzzleGridPosition.x + delta,
                    State.DefaultPuzzleGridPosition.y);
            }
            else
            {
                Game.Session.Puzzle.puzzleGrid.transform.position = State.DefaultPuzzleGridPosition;
            }

            var sequence = _sequence.GetValue<Sequence>(_core);
            TweenUtils.KillTween(sequence, false, true);
            sequence = DOTween.Sequence();
            _sequence.SetValue(_core, sequence);

            if (_gameSaved.GetValue<bool>(_core))
            {
                sequence.Insert(0f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY + 96f, 0.25f, false).SetEase(Ease.InOutSine));
                sequence.Insert(1.25f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY, 0.25f, false).SetEase(Ease.InOutSine));
            }

            sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));
            sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
            sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
            sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
            sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
            sequence.Insert(2.375f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
            {
                Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
            }, 0f, 0.875f).SetEase(Ease.InOutSine));
            sequence.Insert(2f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
            sequence.Insert(2f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
            sequence.Insert(2f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(Game.Persistence.playerFile.locationDefinition.bgYOffset, 1.25f, false).SetEase(Ease.InOutCubic));

            if (Game.Session.Location.AtLocationType(new LocationType[] { LocationType.HUB }))
            {
                sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER), 1f, false).SetEase(Ease.InOutCubic));
            }
            else if (_arriveWithGirls.GetValue<bool>(_core))
            {
                sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                sequence.Insert(2f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));

                var rightOuter = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER);
                var rightSinglePos = (Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER) + rightOuter) / 2f;
                var diff = rightSinglePos - rightOuter - rightSinglePos;
                var rightDrop = Game.Session.gameCanvas.dollRight.dropZone.transform.position;

                if (State.IsSingleDate)
                {
                    if (!_singleDropZone)
                    {
                        _singleDropZone = true;
                        Game.Session.gameCanvas.dollRight.dropZone.transform.position = new Vector3(rightDrop.x - diff.x, rightDrop.y - diff.y, rightDrop.z);
                    }

                    sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(rightSinglePos, 1f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(3f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                }
                else
                {
                    if (_singleDropZone)
                    {
                        _singleDropZone = false;
                        Game.Session.gameCanvas.dollRight.dropZone.transform.position = new Vector3(rightDrop.x + diff.x, rightDrop.y + diff.y, rightDrop.z);
                    }

                    sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER), 1f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(3f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.OUTER), 1f, false).SetEase(Ease.InOutCubic));
                }
            }

            if (_initialArrive.GetValue<bool>(_core))
            {
                sequence.Prepend(Game.Session.gameCanvas.overlayCanvasGroup.DOFade(0f, 1f).SetEase(Ease.Linear));
            }

            if (!Game.Manager.testMode)
            {
                sequence.OnComplete(new TweenCallback(this.OnArriveAnimationsComplete));
                if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                {
                    sequence.timeScale = 3f;
                }
                sequence.Play();
                return false;
            }

            sequence.Complete(false);
            this.ArriveStep();
            return false;
        }
    }

    private void OnArriveAnimationsComplete()
        => m_onArriveAnimationsComplete.Invoke(_core, null);

    public bool DepartStep()
    {
        var stepIndex = _stepIndex.GetValue<int>(_core);

        if (stepIndex != 0
            || !Game.Session.Location.AtLocationType([LocationType.SIM])
            || !State.IsSingleDate)
        {
            return true;
        }

        stepIndex++;
        _stepIndex.SetValue(_core, stepIndex);

        //case stepIndex 1:
        if (Game.Manager.Windows.IsWindowActive(null, true, true))
        {
            Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
            Game.Manager.Windows.HideWindow();
        }
        else
        {
            m_departStep.Invoke(_core, null);
        }

        var uiDoll = Game.Session.gameCanvas.dollRight;

        DialogTriggerDefinition dialogTriggerDefinition;
        if (Game.Persistence.playerFile.locationDefinition.locationType == LocationType.DATE)
        {
            //here we force the focus of the puzzle grid because the initial puzzle 'NextRound' occurs before the current pair is set
            ModInterface.Log.LogInfo("LocationTransitionNormal-Forcing puzzle focus to alt girl for single date");
            _altGirlFocused.SetValue(Game.Session.Puzzle.puzzleStatus, true);

            dialogTriggerDefinition = Game.Session.Location.dtAskDate;
        }
        else
        {
            dialogTriggerDefinition = Game.Session.Location.dtValediction;
        }

        if (dialogTriggerDefinition == null)
        {
            m_departStep.Invoke(_core, null);
        }
        else
        {
            uiDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
            uiDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
        }

        return false;
    }

    private void OnWindowHidden()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
        m_departStep.Invoke(_core, null);
    }

    private void OnValedictionDialogRead(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        m_departStep.Invoke(_core, null);
    }
}
