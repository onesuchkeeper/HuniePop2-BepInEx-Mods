using DG.Tweening;
using Hp2BaseMod;

namespace Hp2BaseMod
{
    public class ModLocationTransitionNormal : LocationTransition
    {
        private int _stepIndex = -1;
        private Sequence _sequence;
        private bool _isAdvancingStep;

        /// <summary>
        /// Resets transition state and cleans up any orphaned event listeners from previous transitions.
        /// </summary>
        private void Reset()
        {
            CleanupPendingStepHooks();
            TweenUtils.KillTween(_sequence, false, false);
            _stepIndex = -1;
            _isAdvancingStep = false;
        }

        /// <summary>
        /// Unsubscribes all potential step-based event listeners to prevent orphaned callbacks 
        /// from firing out of order when a transition step is skipped or forcefully advanced.
        /// </summary>
        private void CleanupPendingStepHooks()
        {
            if (Game.Session?.gameCanvas?.cellphone != null)
            {
                Game.Session.gameCanvas.cellphone.ClosedEvent -= OnCellphoneClosed;
            }

            if (Game.Manager?.Windows != null)
            {
                Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
            }
        }

        public override void DepartStart()
        {
            Reset();
            Game.Session.gameCanvas.bgLocations.nextBg.art.Refresh(
                Game.Persistence.playerFile.locationDefinition, 
                Game.Persistence.playerFile.daytimeElapsed);
            Game.Session.gameCanvas.bgLocations.nextBg.bar.Refresh(
                Game.Persistence.playerFile.locationDefinition, 
                Game.Persistence.playerFile.daytimeElapsed, 
                Game.Persistence.playerFile.girlPairDefinition, 
                Game.Persistence.playerFile.sidesFlipped);
            
            DepartStep();
        }

        /// <summary>
        /// Advances the departure step machine.
        /// </summary>
        private void DepartStep()
        {
            if (_isAdvancingStep)
            {
                return;
            }

            _isAdvancingStep = true;

            // Step 1 runs two tasks in parallel (window hide + state departure dialog), like the base game.
            // Whichever finishes first calls DepartStep while the other is still pending, so leaving step 1
            // must NOT unsubscribe the other task's hook (OnWindowHidden).
            if (_stepIndex != 1)
            {
                CleanupPendingStepHooks();
            }

            _stepIndex++;
            switch (_stepIndex)
            {
                case 0:
                    if (Game.Session.gameCanvas.cellphone.isOpen)
                    {
                        Game.Session.gameCanvas.cellphone.ClosedEvent += OnCellphoneClosed;
                        _isAdvancingStep = false;
                        Game.Session.gameCanvas.cellphone.Close();
                        return;
                    }
                    _isAdvancingStep = false;
                    DepartStep();
                    return;

                case 1:
                {
                    // Base game: hide the window AND read the departure dialog (valediction / ask-date) at the
                    // same time; step 2 is a join, so total wait is max(window hide, dialog) instead of the sum.
                    // Each task calls DepartStep exactly once when it finishes. The first lands on step 2 (barrier),
                    // the second advances to step 3.
                    var windowActive = Game.Manager.Windows.IsWindowActive(null, includeShowing: true, includeHiding: true);
                    if (windowActive)
                    {
                        Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                    }

                    _isAdvancingStep = false;

                    if (windowActive)
                    {
                        Game.Manager.Windows.HideWindow();   // completes asynchronously via OnWindowHidden
                    }
                    else
                    {
                        DepartStep();                        // nothing to wait for: arrives at the barrier now
                    }

                    // Second task. May complete synchronously (states with no dialog), which is fine.
                    ModInterface.GameState.CurrentState.DepartTransition(DepartStep);
                    return;
                }

                case 2:
                    // Join barrier: first finished task lands here and waits for the other.
                    _isAdvancingStep = false;
                    break;

                case 3:
                    Game.Session.Location.isTraveling = true;
                    Game.Session.gameCanvas.dollMiddle.notificationBox.Hide(false);
                    Game.Session.gameCanvas.dollLeft.notificationBox.Hide(false);
                    Game.Session.gameCanvas.dollRight.notificationBox.Hide(false);

                    if (Game.Session.Location.bgMusicLink != null)
                    {
                        if (!Game.Manager.testMode)
                        {
                            Game.Session.Location.bgMusicLink.FadeOut(
                                Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions) ? 1f : 3f);
                        }
                        else
                        {
                            Game.Session.Location.bgMusicLink.FadeOut(0f);
                        }
                    }

                    // Kill active sequence without invoking callbacks to avoid double step advancement
                    TweenUtils.KillTween(_sequence, false, false);

                    var args = new LocationDepartSequenceArgs()
                    {
                        Sequence = DOTween.Sequence()
                    };

                    Game.Session.Location.GetExpansion().NotifyLocationDepartSequence(args);
                    _sequence = args.Sequence ?? DOTween.Sequence();

                    _sequence.Insert(0f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(0.75f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                    {
                        Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                    }, 1f, 0.875f).SetEase(Ease.InOutSine));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                    _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                    _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));
                    _sequence.Insert(3f, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 1130f, 1.5f, false).SetEase(Ease.InOutCubic));

                    if (!Game.Manager.testMode)
                    {
                        _sequence.OnComplete(DepartStep);
                        if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                        {
                            _sequence.timeScale = 3f;
                        }

                        _isAdvancingStep = false;
                        _sequence.Play();
                    }
                    else
                    {
                        _sequence.Complete(withCallbacks: false);
                        _isAdvancingStep = false;
                        DepartStep();
                    }
                    break;

                case 4:
                    _isAdvancingStep = false;
                    DepartureComplete();
                    break;

                default:
                    _isAdvancingStep = false;
                    break;
            }
        }

        public override void ArrivePrep()
        {
            Reset();
            Game.Session.gameCanvas.bgLocations.currentBg.art.Refresh(
                Game.Persistence.playerFile.locationDefinition, 
                Game.Persistence.playerFile.daytimeElapsed);
            Game.Session.gameCanvas.bgLocations.currentBg.bar.Refresh(
                Game.Persistence.playerFile.locationDefinition, 
                Game.Persistence.playerFile.daytimeElapsed, 
                Game.Persistence.playerFile.girlPairDefinition, 
                Game.Persistence.playerFile.sidesFlipped);
            Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
        }

        public override void ArriveStart()
        {
            Game.Session.Location.bgMusicLink = Game.Manager.Audio.Play(
                AudioCategory.MUSIC, 
                (Game.Session.Location.bgMusicOverride != null) ? Game.Session.Location.bgMusicOverride : Game.Persistence.playerFile.locationDefinition.bgMusic);
            ArriveStep();
        }

        /// <summary>
        /// Advances the arrival step machine.
        /// </summary>
        private void ArriveStep()
        {
            if (_isAdvancingStep)
            {
                return;
            }

            _isAdvancingStep = true;

            _stepIndex++;
            switch (_stepIndex)
            {
                case 0:
                    // Kill active sequence without triggering completion callbacks
                    TweenUtils.KillTween(_sequence, false, false);

                    var args = new LocationArriveSequenceArgs()
                    {
                        LeftDollPosition = DollPositionType.OUTER,
                        RightDollPosition = DollPositionType.OUTER,
                        Sequence = DOTween.Sequence()
                    };

                    Game.Session.Location.GetExpansion().NotifyLocationArriveSequence(args);
                    _sequence = args.Sequence ?? DOTween.Sequence();

                    if (_gameSaved)
                    {
                        _sequence.Insert(0f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY + 96f, 0.25f, false).SetEase(Ease.InOutSine));
                        _sequence.Insert(1.25f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY, 0.25f, false).SetEase(Ease.InOutSine));
                    }

                    _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));
                    _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
                    _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
                    _sequence.Insert(2.375f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                    {
                        Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                    }, 0f, 0.875f).SetEase(Ease.InOutSine));
                    _sequence.Insert(2f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(2f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 1.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(2f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(Game.Persistence.playerFile.locationDefinition.bgYOffset, 1.25f, false).SetEase(Ease.InOutCubic));

                    if (ModInterface.GameState.CurrentState.Id == GameStateId.Hub)
                    {
                        _sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER), 1f, false).SetEase(Ease.InOutCubic));
                    }
                    else if (_arriveWithGirls)
                    {
                        _sequence.Insert(2f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(2f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));

                        _sequence.Insert(3f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(args.RightDollPosition), 1f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(3f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(args.LeftDollPosition), 1f, false).SetEase(Ease.InOutCubic));
                    }

                    if (_initialArrive)
                    {
                        _sequence.Prepend(Game.Session.gameCanvas.overlayCanvasGroup.DOFade(0f, 1f).SetEase(Ease.Linear));
                    }

                    if (!Game.Manager.testMode)
                    {
                        _sequence.OnComplete(ArriveStep);
                        if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                        {
                            _sequence.timeScale = 3f;
                        }

                        _isAdvancingStep = false;
                        _sequence.Play();
                    }
                    else
                    {
                        _sequence.Complete(withCallbacks: false);
                        _isAdvancingStep = false;
                        ArriveStep();
                    }

                    break;

                case 1:
                    _isAdvancingStep = false;
                    ArrivalComplete();
                    break;

                default:
                    _isAdvancingStep = false;
                    break;
            }
        }

        private void OnCellphoneClosed()
        {
            Game.Session.gameCanvas.cellphone.ClosedEvent -= OnCellphoneClosed;
            DepartStep();
        }

        private void OnWindowHidden()
        {
            Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
            DepartStep();
        }
    }
}