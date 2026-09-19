// using System.Reflection;
// using DG.Tweening;
// using HarmonyLib;
// using Hp2BaseMod.Extension;

// namespace Hp2BaseMod;

// [Expansion(typeof(LocationTransitionNormal))]
// public partial class ExpandedLocationTransitionNormal
// {
//     [HarmonyPatch(typeof(LocationTransitionNormal))]
//     internal static class Patch
//     {
//         [HarmonyPatch("ArriveStep")]
//         [HarmonyPrefix]
//         private static bool ArriveStep(LocationTransitionNormal __instance)
//             => ExpandedLocationTransitionNormal.Get(__instance).ArriveStep_Prefix();

//         [HarmonyPatch("DepartStep")]
//         [HarmonyPrefix]
//         private static bool DepartStep(LocationTransitionNormal __instance)
//             => ExpandedLocationTransitionNormal.Get(__instance).DepartStep_Prefix();
//     }

//     private static FieldInfo f_gameSaved = AccessTools.Field(typeof(LocationTransition), "_gameSaved");
//     private static FieldInfo f_arriveWithGirls = AccessTools.Field(typeof(LocationTransition), "_arriveWithGirls");
//     private static FieldInfo f_initialArrive = AccessTools.Field(typeof(LocationTransition), "_initialArrive");
//     private static MethodInfo m_ArrivalComplete = AccessTools.Method(typeof(LocationTransition), "ArrivalComplete");

//     private bool ArriveStep_Prefix()
//     {
//         // override arrive step 1 to allow additions to the sequence before it plays
//         var nextStepIndex = f_stepIndex.GetValue<int>(_core) + 1;
//         _stepIndex = nextStepIndex;

//         if (nextStepIndex != 0)
//         {
//             if (nextStepIndex != 1)
//             {
//                 return false;
//             }

//             m_ArrivalComplete.Invoke(_core, null);
//             return false;
//         }
//         else
//         {
//             var sequence = f_sequence.GetValue<Sequence>(_core);
//             TweenUtils.KillTween(sequence, false, true);

//             //Notify
//             var args = new LocationArriveSequenceArgs()
//             {
//                 LeftDollPosition = DollPositionType.OUTER,
//                 RightDollPosition = DollPositionType.OUTER,
//                 Sequence = DOTween.Sequence()
//             };

//             Game.Session.Location.GetExpansion().NotifyLocationArriveSequence(args);

//             sequence = args.Sequence ?? DOTween.Sequence();
//             _sequence = sequence;

//             if (f_gameSaved.GetValue<bool>(_core))
//             {
//                 sequence.Insert(0f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY + 96f, 0.25f, false).SetEase(Ease.InOutSine));
//                 sequence.Insert(1.25f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY, 0.25f, false).SetEase(Ease.InOutSine));
//             }

//             sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));
//             sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
//             sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
//             sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
//             sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
//             sequence.Insert(2.375f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
//             {
//                 Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
//             }, 0f, 0.875f).SetEase(Ease.InOutSine));
//             sequence.Insert(2f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
//             sequence.Insert(2f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
//             sequence.Insert(2f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(Game.Persistence.playerFile.locationDefinition.bgYOffset, 1.25f, false).SetEase(Ease.InOutCubic));

//             if (ModInterface.GameState.CurrentState.Id == GameStateId.Hub)
//             {
//                 sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
//                 sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER), 1f, false).SetEase(Ease.InOutCubic));
//             }
//             else if (f_arriveWithGirls.GetValue<bool>(_core))
//             {
//                 sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
//                 sequence.Insert(2f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));

//                 sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(args.RightDollPosition), 1f, false).SetEase(Ease.InOutCubic));
//                 sequence.Insert(3f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(args.LeftDollPosition), 1f, false).SetEase(Ease.InOutCubic));
//             }

//             if (f_initialArrive.GetValue<bool>(_core))
//             {
//                 sequence.Prepend(Game.Session.gameCanvas.overlayCanvasGroup.DOFade(0f, 1f).SetEase(Ease.Linear));
//             }

//             if (!Game.Manager.testMode)
//             {
//                 sequence.OnComplete(new TweenCallback(OnArriveAnimationsComplete));
//                 if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
//                 {
//                     sequence.timeScale = 3f;
//                 }
//                 sequence.Play();
//                 return false;
//             }

//             sequence.Complete(false);
//             this.ArriveStep();
//             return false;
//         }
//     }

//     private bool DepartStep_Prefix()
//     {
//         // override depart step 1 at sim locations to notify random doll selection
//         var stepIndex = f_stepIndex.GetValue<int>(_core);

//         switch (stepIndex)
//         {
//             case 0:
//                 {
//                     if (ModInterface.GameState.CurrentState.Id != GameStateId.Sim)
//                     {
//                         return true;
//                     }

//                     _stepIndex = stepIndex + 1;

//                     if (Game.Manager.Windows.IsWindowActive(null, true, true))
//                     {
//                         Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden_Hook;
//                         Game.Manager.Windows.HideWindow();
//                     }
//                     else
//                     {
//                         m_DepartStep.Invoke(_core, null);
//                     }

//                     var args = new RandomDollSelectedArgs();
//                     ModInterface.Events.NotifyRandomDollSelected(args);
//                     var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

//                     DialogTriggerDefinition dialogTriggerDefinition;
//                     if (Game.Persistence.playerFile.locationDefinition.locationType == LocationType.DATE)
//                     {
//                         dialogTriggerDefinition = Game.Session.Location.dtAskDate;
//                     }
//                     else
//                     {
//                         dialogTriggerDefinition = Game.Session.Location.dtValediction;
//                     }

//                     if (dialogTriggerDefinition != null)
//                     {
//                         ModInterface.Log.Warning("Valediction Dialog Setup");
//                         uiDoll.DialogBoxHiddenEvent += OnValedictionDialogRead_Hook;
//                         uiDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
//                         return false;
//                     }

//                     m_DepartStep.Invoke(_core, null);
//                     return false;
//                 }
//             case 2:
//                 {
//                     _stepIndex = stepIndex + 1;

//                     Game.Session.Location.isTraveling = true;
//                     Game.Session.gameCanvas.dollMiddle.notificationBox.Hide(false);
//                     Game.Session.gameCanvas.dollLeft.notificationBox.Hide(false);
//                     Game.Session.gameCanvas.dollRight.notificationBox.Hide(false);

//                     if (Game.Session.Location.bgMusicLink != null)
//                     {
//                         if (!Game.Manager.testMode)
//                         {
//                             Game.Session.Location.bgMusicLink.FadeOut((!Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions)) ? 3 : 1);
//                         }
//                         else
//                         {
//                             Game.Session.Location.bgMusicLink.FadeOut(0f);
//                         }
//                     }

//                     var sequence = f_sequence.GetValue<Sequence>(_core);
//                     TweenUtils.KillTween(sequence, false, true);

//                     //Notify
//                     var args = new LocationDepartSequenceArgs()
//                     {
//                         Sequence = DOTween.Sequence()
//                     };

//                     Game.Session.Location.GetExpansion().NotifyLocationDepartSequence(args);

//                     sequence = args.Sequence ?? DOTween.Sequence();
//                     _sequence = sequence;

//                     sequence.Insert(0f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(0.75f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
//                     {
//                         Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
//                     }, 1f, 0.875f).SetEase(Ease.InOutSine));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
//                     sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
//                     sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
//                     sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));
//                     sequence.Insert(3f, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 1130f, 1.5f, false).SetEase(Ease.InOutCubic));
//                     if (!Game.Manager.testMode)
//                     {
//                         sequence.OnComplete(new TweenCallback(OnDepartAnimationsComplete));
//                         if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
//                         {
//                             sequence.timeScale = 3f;
//                         }
//                         sequence.Play();
//                         return false;
//                     }
//                     sequence.Complete(false);
//                     this.DepartStep_Prefix();
//                     return false;
//                 }
//         }
//         return true;
//     }

//     private void OnWindowHidden_Hook()
//     {
//         Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden_Hook;
//         m_DepartStep.Invoke(_core, null);
//     }

//     private void OnValedictionDialogRead_Hook(UiDoll doll)
//     {
//         doll.DialogBoxHiddenEvent -= OnValedictionDialogRead_Hook;
//         m_DepartStep.Invoke(_core, null);
//     }
// }
