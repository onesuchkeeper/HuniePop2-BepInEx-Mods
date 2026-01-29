using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

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

    private static readonly FieldInfo f_stepIndex = AccessTools.Field(typeof(LocationTransitionNormal), "_stepIndex");
    private static readonly FieldInfo f_gameSaved = AccessTools.Field(typeof(LocationTransitionNormal), "_gameSaved");
    private static readonly FieldInfo f_arriveWithGirls = AccessTools.Field(typeof(LocationTransitionNormal), "_arriveWithGirls");
    private static readonly FieldInfo f_initialArrive = AccessTools.Field(typeof(LocationTransitionNormal), "_initialArrive");
    private static readonly FieldInfo f_sequence = AccessTools.Field(typeof(LocationTransitionNormal), "_sequence");
    private static readonly MethodInfo m_departStep = AccessTools.Method(typeof(LocationTransitionNormal), "DepartStep");
    private static readonly MethodInfo m_arrivalComplete = AccessTools.Method(typeof(LocationTransitionNormal), "ArrivalComplete");
    private static readonly MethodInfo m_onArriveAnimationsComplete = AccessTools.Method(typeof(LocationTransitionNormal), "OnArriveAnimationsComplete");
    private static readonly MethodInfo m_onDepartAnimationsComplete = AccessTools.Method(typeof(LocationTransitionNormal), "OnDepartAnimationsComplete");

    private readonly LocationTransitionNormal _core;
    public ExpandedLocationTransitionNormal(LocationTransitionNormal core)
    {
        _core = core;
    }

    public bool ArriveStep()
    {
        // override arrive step 1 to allow additions to the sequence before it plays
        var stepIndex = f_stepIndex.GetValue<int>(_core);
        stepIndex++;
        f_stepIndex.SetValue(_core, stepIndex);

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
            var sequence = f_sequence.GetValue<Sequence>(_core);
            TweenUtils.KillTween(sequence, false, true);

            //Notify
            var args = new LocationArriveSequenceArgs()
            {
                LeftDollPosition = DollPositionType.OUTER,
                RightDollPosition = DollPositionType.OUTER,
                Sequence = DOTween.Sequence()
            };

            ModInterface.Events.NotifyLocationArriveSequence(args);

            sequence = args.Sequence ?? DOTween.Sequence();
            f_sequence.SetValue(_core, sequence);

            if (f_gameSaved.GetValue<bool>(_core))
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

            if (Game.Session.Location.AtLocationType([LocationType.HUB]))
            {
                sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER), 1f, false).SetEase(Ease.InOutCubic));
            }
            else if (f_arriveWithGirls.GetValue<bool>(_core))
            {
                sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                sequence.Insert(2f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));

                sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(args.RightDollPosition), 1f, false).SetEase(Ease.InOutCubic));
                sequence.Insert(3f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(args.LeftDollPosition), 1f, false).SetEase(Ease.InOutCubic));
            }

            if (f_initialArrive.GetValue<bool>(_core))
            {
                sequence.Prepend(Game.Session.gameCanvas.overlayCanvasGroup.DOFade(0f, 1f).SetEase(Ease.Linear));
            }

            if (!Game.Manager.testMode)
            {
                sequence.OnComplete(new TweenCallback(OnArriveAnimationsComplete));
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
        // override depart step 1 at sim locations to notify random doll selection
        var stepIndex = f_stepIndex.GetValue<int>(_core);

        switch (stepIndex)
        {
            case 0:
                {
                    if (!Game.Session.Location.AtLocationType([LocationType.SIM]))
                    {
                        return true;
                    }

                    stepIndex++;
                    f_stepIndex.SetValue(_core, stepIndex);

                    if (Game.Manager.Windows.IsWindowActive(null, true, true))
                    {
                        Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                        Game.Manager.Windows.HideWindow();
                    }
                    else
                    {
                        m_departStep.Invoke(_core, null);
                    }

                    var args = new RandomDollSelectedArgs();
                    ModInterface.Events.NotifyRandomDollSelected(args);
                    var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

                    DialogTriggerDefinition dialogTriggerDefinition;
                    if (Game.Persistence.playerFile.locationDefinition.locationType == LocationType.DATE)
                    {
                        dialogTriggerDefinition = Game.Session.Location.dtAskDate;
                    }
                    else
                    {
                        dialogTriggerDefinition = Game.Session.Location.dtValediction;
                    }

                    if (dialogTriggerDefinition != null)
                    {
                        ModInterface.Log.Warning("Valediction Dialog Setup");
                        uiDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
                        uiDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
                        return false;
                    }

                    m_departStep.Invoke(_core, null);
                    return false;
                }
            case 2:
                {
                    stepIndex++;
                    f_stepIndex.SetValue(_core, stepIndex);

                    Game.Session.Location.isTraveling = true;
                    Game.Session.gameCanvas.dollMiddle.notificationBox.Hide(false);
                    Game.Session.gameCanvas.dollLeft.notificationBox.Hide(false);
                    Game.Session.gameCanvas.dollRight.notificationBox.Hide(false);

                    if (Game.Session.Location.bgMusicLink != null)
                    {
                        if (!Game.Manager.testMode)
                        {
                            Game.Session.Location.bgMusicLink.FadeOut((!Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions)) ? 3 : 1);
                        }
                        else
                        {
                            Game.Session.Location.bgMusicLink.FadeOut(0f);
                        }
                    }

                    var sequence = f_sequence.GetValue<Sequence>(_core);
                    TweenUtils.KillTween(sequence, false, true);

                    //Notify
                    var args = new LocationDepartSequenceArgs()
                    {
                        Sequence = DOTween.Sequence()
                    };

                    ModInterface.Events.NotifyLocationDepartSequence(args);

                    sequence = args.Sequence ?? DOTween.Sequence();
                    f_sequence.SetValue(_core, sequence);

                    sequence.Insert(0f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(0.75f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                    {
                        Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                    }, 1f, 0.875f).SetEase(Ease.InOutSine));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                    sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                    sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));
                    sequence.Insert(3f, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 1130f, 1.5f, false).SetEase(Ease.InOutCubic));
                    if (!Game.Manager.testMode)
                    {
                        sequence.OnComplete(new TweenCallback(OnDepartAnimationsComplete));
                        if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                        {
                            sequence.timeScale = 3f;
                        }
                        sequence.Play();
                        return false;
                    }
                    sequence.Complete(false);
                    this.DepartStep();
                    return false;
                }
        }
        return true;
    }

    private void OnDepartAnimationsComplete()
        => m_onDepartAnimationsComplete.Invoke(_core, null);

    private void OnWindowHidden()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
        m_departStep.Invoke(_core, null);
    }

    private void OnValedictionDialogRead(UiDoll doll)
    {
        ModInterface.Log.Warning("Valediction Dialog Read");
        doll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        m_departStep.Invoke(_core, null);
    }
}
