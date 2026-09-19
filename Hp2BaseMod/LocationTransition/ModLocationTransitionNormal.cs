using DG.Tweening;

namespace Hp2BaseMod;

/// <summary>
/// A reimplementation of <see cref="LocationTransitionNormal"> that
/// hooks into the mod state system. Completly replaces and depreciates
/// the base game's <see cref="LocationTransitionNormal">
/// </summary>

//TODO, add type depreciation
//[Deprecates(typeof(LocationTransitionNormal))]
public class ModLocationTransitionNormal : LocationTransition
{
    private int _stepIndex;
    private Sequence _sequence;

    private void Reset()
    {
        _stepIndex = -1;
    }

    public override void DepartStart()
    {
        Reset();
        Game.Session.gameCanvas.bgLocations.nextBg.art.Refresh(Game.Persistence.playerFile.locationDefinition, Game.Persistence.playerFile.daytimeElapsed);
        Game.Session.gameCanvas.bgLocations.nextBg.bar.Refresh(Game.Persistence.playerFile.locationDefinition, Game.Persistence.playerFile.daytimeElapsed, Game.Persistence.playerFile.girlPairDefinition, Game.Persistence.playerFile.sidesFlipped);
        DepartStep();
    }

    private void DepartStep()
    {
        _stepIndex++;
        switch (_stepIndex)
        {
            case 0:
                if (Game.Session.gameCanvas.cellphone.isOpen)
                {
                    Game.Session.gameCanvas.cellphone.ClosedEvent += OnCellphoneClosed;
                    Game.Session.gameCanvas.cellphone.Close();
                }
                else
                {
                    DepartStep();
                }

                break;
            case 1:
                if (Game.Manager.Windows.IsWindowActive(null, includeShowing: true, includeHiding: true))
                {
                    Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                    Game.Manager.Windows.HideWindow();
                }
                else
                {
                    DepartStep();
                }
                break;
            case 2:
                ModInterface.GameState.CurrentState.DepartTransition(DepartStep);
                break;
            case 3:
                Game.Session.Location.isTraveling = true;
                Game.Session.gameCanvas.dollMiddle.notificationBox.Hide();
                Game.Session.gameCanvas.dollLeft.notificationBox.Hide();
                Game.Session.gameCanvas.dollRight.notificationBox.Hide();
                if (Game.Session.Location.bgMusicLink != null)
                {
                    if (!Game.Manager.testMode)
                    {
                        Game.Session.Location.bgMusicLink.FadeOut(Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions) ? 1 : 3);
                    }
                    else
                    {
                        Game.Session.Location.bgMusicLink.FadeOut(0f);
                    }
                }

                TweenUtils.KillTween(_sequence);

                var args = new LocationDepartSequenceArgs()
                {
                    Sequence = DOTween.Sequence()
                };

                Game.Session.Location.GetExpansion().NotifyLocationDepartSequence(args);

                _sequence = args.Sequence ?? DOTween.Sequence();

                _sequence.Insert(0f, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0f, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.HIDDEN), 1f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(0.75f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                {
                    Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                }, 1f, 0.875f).SetEase(Ease.InOutSine));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                _sequence.Insert(0.75f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
                _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
                _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));
                _sequence.Insert(3f, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 1130f, 1.5f).SetEase(Ease.InOutCubic));
                if (!Game.Manager.testMode)
                {
                    _sequence.OnComplete(DepartStep);
                    if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                    {
                        _sequence.timeScale = 3f;
                    }

                    _sequence.Play();
                }
                else
                {
                    _sequence.Complete(withCallbacks: false);
                    DepartStep();
                }

                break;
            case 4:
                DepartureComplete();
                break;
        }
    }

    public override void ArrivePrep()
    {
        Reset();
        Game.Session.gameCanvas.bgLocations.currentBg.art.Refresh(Game.Persistence.playerFile.locationDefinition, Game.Persistence.playerFile.daytimeElapsed);
        Game.Session.gameCanvas.bgLocations.currentBg.bar.Refresh(Game.Persistence.playerFile.locationDefinition, Game.Persistence.playerFile.daytimeElapsed, Game.Persistence.playerFile.girlPairDefinition, Game.Persistence.playerFile.sidesFlipped);
        Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
    }

    public override void ArriveStart()
    {
        Game.Session.Location.bgMusicLink = Game.Manager.Audio.Play(AudioCategory.MUSIC, (Game.Session.Location.bgMusicOverride != null) ? Game.Session.Location.bgMusicOverride : Game.Persistence.playerFile.locationDefinition.bgMusic);
        ArriveStep();
    }

    private void ArriveStep()
    {
        _stepIndex++;
        switch (_stepIndex)
        {
            case 0:
                TweenUtils.KillTween(_sequence);

                //Notify
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
                    _sequence.Insert(0f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY + 96f, 0.25f).SetEase(Ease.InOutSine));
                    _sequence.Insert(1.25f, Game.Session.gameCanvas.bgLocations.savedNotification.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.savedNotificationOrigY, 0.25f).SetEase(Ease.InOutSine));
                }

                _sequence.Insert(1.75f, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));
                _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
                _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
                _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
                _sequence.Insert(1.5f, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
                _sequence.Insert(2.375f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                {
                    Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                }, 0f, 0.875f).SetEase(Ease.InOutSine));
                _sequence.Insert(2f, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(2f, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 1.25f).SetEase(Ease.InOutCubic));
                _sequence.Insert(2f, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(Game.Persistence.playerFile.locationDefinition.bgYOffset, 1.25f).SetEase(Ease.InOutCubic));

                //TODO move this to states.
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

                    _sequence.Play();
                }
                else
                {
                    _sequence.Complete(withCallbacks: false);
                    ArriveStep();
                }

                break;
            case 1:
                ArrivalComplete();
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