using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace HuniePopUltimate;

public class LocationTransitionFakeOut
{
    // location transition
    public delegate void LocationTransitionDelegate();

    protected bool _arriveWithGirls;

    // protected bool _gameSaved;

    public void Arrive(bool arriveWithGirls)
    {
        _arriveWithGirls = arriveWithGirls;
        ArriveStart();
    }

    protected void ArrivalComplete()
    {
        _arriveWithGirls = false;
        OnArrivalComplete();
    }

    private void Reset()
    {
        _stepIndex = -1;
    }

    public void DepartStart()
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
        switch (_stepIndex)
        {
            case 0:
                if (Game.Session.gameCanvas.cellphone.isOpen)
                {
                    Game.Session.gameCanvas.cellphone.ClosedEvent += OnCellphoneClosed;
                    Game.Session.gameCanvas.cellphone.Close();
                    return;
                }
                DepartStep();
                return;
            case 1:
                {
                    // don't use AtLocationType to avoid the override for the depart
                    var currentLocType = Game.Session.Location.currentLocation.locationType;
                    if (!(currentLocType == LocationType.SIM || currentLocType == LocationType.HUB))
                    {
                        DepartStep();
                        DepartStep();
                        return;
                    }

                    if (Game.Manager.Windows.IsWindowActive(null, true, true))
                    {
                        Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
                        Game.Manager.Windows.HideWindow();
                    }
                    else
                    {
                        DepartStep();
                    }

                    var dollChoice = MathUtils.RandomBool();
                    UiDoll uiDoll = Game.Session.gameCanvas.GetDoll(dollChoice);
                    if (uiDoll.girlDefinition.specialCharacter)
                    {
                        uiDoll = Game.Session.gameCanvas.GetDoll(!dollChoice);
                    }

                    if (Game.Session.Location.AtLocationType([LocationType.HUB]))
                    {
                        uiDoll = Game.Session.gameCanvas.GetDoll(DollOrientationType.RIGHT);
                    }

                    DialogTriggerDefinition dialogTriggerDefinition;
                    if (Game.Session.Location.AtLocationType([LocationType.HUB]))
                    {
                        dialogTriggerDefinition = Game.Session.Hub.GetValediction();
                    }
                    else if (_fakeLoc.locationType == LocationType.DATE)
                    {
                        dialogTriggerDefinition = Game.Session.Location.dtAskDate;
                    }
                    else
                    {
                        dialogTriggerDefinition = Game.Session.Location.dtValediction;
                    }

                    if (dialogTriggerDefinition != null)
                    {
                        uiDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
                        uiDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
                        return;
                    }

                    DepartStep();
                    return;
                }
            case 2:
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

                if (!Game.Manager.testMode)
                {
                    _sequence.OnComplete(new TweenCallback(OnDepartAnimationsComplete));
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
                OnDepartureComplete();
                break;
            default:
                return;
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

    private void OnValedictionDialogRead(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        DepartStep();
    }

    private void OnDialogBoxHidden(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnDialogBoxHidden;
        DepartStep();
    }

    private void OnDepartAnimationsComplete()
    {
        DepartStep();
    }

    public void ArrivePrep()
    {
        Reset();
        Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;
    }

    public void ArriveStart()
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
                        _sequence.OnComplete(new TweenCallback(OnArriveAnimationsComplete));
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

    private void OnArriveAnimationsComplete()
    {
        ArriveStep();
    }

    private void OnGreetingQueueEmpty()
    {
        Game.Session.Dialog.DialogQueueEmptyEvent -= OnGreetingQueueEmpty;
        ArriveStep();
    }

    private int _stepIndex;

    private Sequence _sequence;

    private static FieldInfo f_isTraveling = AccessTools.Field(typeof(LocationManager), "_isTraveling");
    private static FieldInfo f_bgMusicOverride = AccessTools.Field(typeof(LocationManager), "_bgMusicOverride");
    private static FieldInfo f_arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static FieldInfo f_isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static FieldInfo f_currentLocation = AccessTools.Field(typeof(LocationManager), "_currentLocation");
    private static FieldInfo f_currentGirlPair = AccessTools.Field(typeof(LocationManager), "_currentGirlPair");
    private static FieldInfo f_currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");

    private static MethodInfo m_OnLocationSettled = AccessTools.Method(typeof(LocationManager), "OnLocationSettled");

    private void OnDepartureComplete()
    {
        Game.Session.Location.ResetDolls(true);
        Arrive(Game.Persistence.playerFile.locationDefinition, Game.Persistence.playerFile.girlPairDefinition, Game.Persistence.playerFile.sidesFlipped);
    }

    private void OnArrivalComplete()
    {
        var locationManager = Game.Session.Location;

        f_isTraveling.SetValue(locationManager, false);
        f_bgMusicOverride.SetValue(locationManager, null);
        Game.Session.gameCanvas.overlayCanvasGroup.blocksRaycasts = false;
        var arrivalCutscene = f_arrivalCutscene.GetValue<CutsceneDefinition>(locationManager);

        if (arrivalCutscene != null)
        {
            ModInterface.Log.Message($"has arrival cutscene {arrivalCutscene.name}");
            Game.Session.Cutscenes.CutsceneCompleteEvent += OnCutsceneComplete;
            Game.Session.Cutscenes.StartCutscene(arrivalCutscene, null);
            return;
        }

        m_OnLocationSettled.Invoke(Game.Session.Location, []);
    }

    private void OnCutsceneComplete()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnCutsceneComplete;
        ModInterface.Log.Message("Settling Via Fakeout");
        m_OnLocationSettled.Invoke(Game.Session.Location, []);
    }

    private LocationDefinition _fakeLoc;

    public void Depart(LocationDefinition locationDef, GirlPairDefinition girlPairDef, bool sidesFlipped = false)
    {
        Game.Session.Puzzle.puzzleGrid.ailmentsContainer.Deactivate();
        Game.Session.gameCanvas.overlayCanvasGroup.blocksRaycasts = true;
        f_isLocked.SetValue(Game.Session.Location, true);
        _fakeLoc = locationDef;
        if (girlPairDef != null)
        {
            Game.Persistence.playerFile.girlPairDefinition = girlPairDef;
            Game.Persistence.playerFile.sidesFlipped = sidesFlipped;
        }
        else
        {
            Game.Persistence.playerFile.girlPairDefinition = null;
            Game.Persistence.playerFile.sidesFlipped = false;
        }

        DepartStart();
    }

    public void Arrive(LocationDefinition locationDef, GirlPairDefinition girlPairDef, bool sidesFlipped)
    {
        var locationManager = Game.Session.Location;

        Game.Persistence.playerFile.locationDefinition = locationDef;
        Game.Persistence.playerFile.girlPairDefinition = girlPairDef;
        Game.Persistence.playerFile.sidesFlipped = sidesFlipped;

        var currentLocation = Game.Persistence.playerFile.locationDefinition;

        // Make a fake sim location if the current loc isn't already one.
        if (currentLocation.locationType != LocationType.SIM)
        {
            currentLocation = GameObject.Instantiate(currentLocation);
            currentLocation.locationType = LocationType.SIM;
        }

        f_currentLocation.SetValue(locationManager, currentLocation);

        var currentGirlPair = Game.Persistence.playerFile.girlPairDefinition;
        f_currentGirlPair.SetValue(locationManager, currentGirlPair);

        f_currentSidesFlipped.SetValue(locationManager, Game.Persistence.playerFile.sidesFlipped);
        ArrivePrep();

        if (currentGirlPair != null)
        {
            Game.Session.Puzzle.puzzleStatus.Reset(Game.Session.Location.currentGirlLeft, Game.Session.Location.currentGirlRight);
            Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.staminaFreeze = -1;
            Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.staminaFreeze = -1;
        }
        else
        {
            Game.Session.Puzzle.puzzleStatus.Clear();
        }

        Game.Session.Location.ResetDolls(currentGirlPair == Game.Session.Puzzle.bossGirlPairDefinition);
        Game.Session.Hub.PrepHub();

        Game.Session.gameCanvas.header.rectTransform.anchoredPosition = new Vector2(
            ModInterface.State.CellphoneOnLeft
                ? Game.Session.gameCanvas.header.xValues.y
                : Game.Session.gameCanvas.header.xValues.x,
            Game.Session.gameCanvas.header.rectTransform.anchoredPosition.y);

        Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition = new Vector2(
            ModInterface.State.CellphoneOnLeft
                ? Game.Session.gameCanvas.cellphone.xValues.y
                : Game.Session.gameCanvas.cellphone.xValues.x,
            Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition.y);

        Game.Session.gameCanvas.header.Refresh(true);
        Game.Session.gameCanvas.cellphone.Refresh(true);

        CutsceneDefinition arrivalCutscene = null;
        f_arrivalCutscene.SetValue(locationManager, arrivalCutscene);

        if (currentGirlPair != null && !currentGirlPair.specialPair)
        {
            PlayerFileGirl playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(currentGirlPair.girlDefinitionOne);
            if (!playerFileGirl.playerMet)
            {
                playerFileGirl.playerMet = true;
            }
            PlayerFileGirl playerFileGirl2 = Game.Persistence.playerFile.GetPlayerFileGirl(currentGirlPair.girlDefinitionTwo);
            if (!playerFileGirl2.playerMet)
            {
                playerFileGirl2.playerMet = true;
            }
            PlayerFileGirlPair playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);
            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                playerFileGirlPair.RelationshipLevelUp();
                if (currentGirlPair.introductionPair)
                {
                    arrivalCutscene = locationManager.cutsceneMeetingIntro;
                }
                else
                {
                    arrivalCutscene = locationManager.cutsceneMeeting;
                }
                f_arrivalCutscene.SetValue(locationManager, arrivalCutscene);
            }
        }

        Arrive((currentGirlPair != null || !Game.Session.Puzzle.puzzleStatus.isEmpty) && arrivalCutscene == null);
    }
}
