using DG.Tweening;
using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// A transition that begins moving to a fake location, then abruptly returns to
/// the next location
/// </summary>
public class LocationTransitionFakeOut : LocationTransition
{
    private int _stepIndex;
    private Sequence _sequence;
    private LocationDefinition _fakeLoc;
    private readonly RelativeId _nextStateId;

    public LocationTransitionFakeOut(LocationDefinition fakeLoc, RelativeId gameStateId)
    {
        _fakeLoc = fakeLoc;
        _nextStateId = gameStateId;
    }

    private void Reset()
    {
        _stepIndex = -1;
    }

    public override void DepartStart()
    {
        Reset();
        Game.Persistence.playerFile.daytimeElapsed--;
        Game.Session.gameCanvas.bgLocations.nextBg.art.Refresh(_fakeLoc, Game.Persistence.playerFile.daytimeElapsed);
        Game.Session.gameCanvas.bgLocations.nextBg.bar.Refresh(_fakeLoc, Game.Persistence.playerFile.daytimeElapsed, Game.Persistence.playerFile.girlPairDefinition, Game.Persistence.playerFile.sidesFlipped);
        DepartStep();
    }

    private void DepartStep()
    {
        _stepIndex++;
        ModInterface.Log.Message($"Fakeout Depart {_stepIndex}");
        switch (_stepIndex)
        {
            //Close Cellphone
            case 0:
                if (Game.Session.gameCanvas.cellphone.isOpen)
                {
                    Game.Session.gameCanvas.cellphone.ClosedEvent += OnDepartCellphoneClosed;
                    Game.Session.gameCanvas.cellphone.Close();
                    return;
                }
                DepartStep();
                return;
            //Close Window
            case 1:
                if (Game.Manager.Windows.IsWindowActive(null, true, true))
                {
                    Game.Manager.Windows.WindowHiddenEvent += OnDepartWindowHidden;
                    Game.Manager.Windows.HideWindow();
                    break;
                }
                DepartStep();
                return;
            //Depart State
            case 2:
                ModInterface.GameState.CurrentState.DepartTransition(_nextStateId, DepartStep);
                break;
            //Animate
            case 3:
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

                TweenUtils.KillTween(_sequence, false, true);
                _sequence = DOTween.Sequence();
                var time = 0f;

                // hide dolls
                _sequence.Insert(time, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.HIDDEN), 1f, false).SetEase(Ease.InOutCubic));
                time += 0.75f;

                // hide ui
                _sequence.Insert(time, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                {
                    Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                }, 1f, 0.875f).SetEase(Ease.InOutSine));

                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
                time += 1f;

                // show bar
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f, false).SetEase(Ease.InOutCubic));
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));
                time += 1.25f;

                // Fake out
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.rectTransform
                    .DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 280, 0.375f, false)
                    .SetEase(Ease.InOutCubic))
                    .AppendCallback(() => Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject));

                time += 0.5f;
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.rectTransform.DOShakePosition(0.5f, 20f, 20, 90f, false, false));
                time += 0.25f;
                _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y, 0.375f, false).SetEase(Ease.InOutCubic));

                //Play
                if (!Game.Manager.testMode)
                {
                    _sequence.OnComplete(new TweenCallback(DepartStep));
                    if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                    {
                        _sequence.timeScale = 3f;
                    }
                    _sequence.Play();
                    return;
                }

                _sequence.Complete(false);
                DepartStep();
                return;
            case 4:
                DepartureComplete();
                break;
            default:
                return;
        }
    }

    public override void ArrivePrep()
    {
        Reset();
        Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
    }

    public override void ArriveStart()
    {
        Game.Session.Location.bgMusicLink = Game.Manager.Audio.Play(AudioCategory.MUSIC, (Game.Session.Location.bgMusicOverride != null) ? Game.Session.Location.bgMusicOverride : Game.Persistence.playerFile.locationDefinition.bgMusic, null);
        ArriveStep();
    }

    private void ArriveStep()
    {
        _stepIndex++;

        switch (_stepIndex)
        {
            case 0:
                {
                    TweenUtils.KillTween(_sequence, false, true);
                    _sequence = DOTween.Sequence();
                    var time = 0f;
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.25f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1f).SetEase(Ease.InOutSine));
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1f).SetEase(Ease.InOutSine));
                    time += 0.25f;
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));
                    _sequence.Insert(time + 0.2f, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
                    {
                        Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
                    }, 0f, 0.4f).SetEase(Ease.InOutSine));
                    time += 0.1f;
                    _sequence.Insert(time, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 0.2f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(time, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 0.2f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(time, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(Game.Persistence.playerFile.locationDefinition.bgYOffset, 0.2f, false).SetEase(Ease.InOutCubic));
                    _sequence.Insert(time + 0.05f, Game.Session.gameCanvas.bgLocations.rectTransform.DOShakePosition(0.2f, 10f, 20, 90f, false, false));

                    if (_arriveWithGirls)
                    {
                        _sequence.Insert(time, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(time, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.y, 1.25f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(time + 1, Game.Session.gameCanvas.dollRight.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER), 1f, false).SetEase(Ease.InOutCubic));
                        _sequence.Insert(time + 1, Game.Session.gameCanvas.dollLeft.slideLayer.DOAnchorPos(Game.Session.gameCanvas.dollLeft.GetPositionByType(DollPositionType.OUTER), 1f, false).SetEase(Ease.InOutCubic));
                    }

                    if (Game.Manager.testMode)
                    {
                        _sequence.Complete(false);
                        ArriveStep();
                    }
                    else
                    {
                        _sequence.OnComplete(new TweenCallback(ArriveStep));
                        if (Game.Persistence.playerData.unlockedCodes.Contains(Game.Session.Location.codeDefQuickTransitions))
                        {
                            _sequence.timeScale = 3f;
                        }
                        _sequence.Play();
                    }
                }
                break;
            case 1:
                ArrivalComplete();
                break;
        }
    }

    private void OnDepartCellphoneClosed()
    {
        Game.Session.gameCanvas.cellphone.ClosedEvent -= OnDepartCellphoneClosed;
        DepartStep();
    }

    private void OnDepartWindowHidden()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnDepartWindowHidden;
        DepartStep();
    }
}
