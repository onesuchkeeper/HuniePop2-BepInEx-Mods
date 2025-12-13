using System;
using System.Linq;
using DG.Tweening;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.ModGameData;

/// <summary>
/// A cutscene step that changes location without any of the coupling of
/// <see cref="LocationManager"/>. This changes location visually and in <see cref="Game.Persistence.PlayerFile.locationDefinition"/> 
/// It does not handle any other transitions like moving dolls, triggering dialogue or toggling menus.
/// </summary>
public class ChangeLocationCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    public class Info : IGameDefinitionInfo<CutsceneStepSubDefinition>
    {
        private RelativeId[] _locationIdPool;

        public Info(params RelativeId[] locationIdPool)
        {
            _locationIdPool = locationIdPool;
        }

        public void SetData(ref CutsceneStepSubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            _ = gameData;
            _ = assetProvider;
            _ = insertStyle;
            def = new ChangeLocationCutsceneStep(_locationIdPool);
        }

        public void RequestInternals(AssetProvider assetProvider)
        {
            // no internals
        }
    }

    public event CutsceneStepComplete Complete;

    private readonly RelativeId[] _locationIdPool;
    private Sequence _sequence;

    public ChangeLocationCutsceneStep(params RelativeId[] locationIdPool)
    {
        _locationIdPool = locationIdPool ?? throw new ArgumentNullException(nameof(locationIdPool));
    }

    public void Act()
    {
        var locationDef = _locationIdPool.Any()
            ? ModInterface.GameData.GetLocation(_locationIdPool.GetRandom())
            : Game.Persistence.playerFile.locationDefinition;

        Game.Persistence.playerFile.locationDefinition = locationDef;

        // Depart
        Game.Session.gameCanvas.bgLocations.nextBg.art.Refresh(locationDef, Game.Persistence.playerFile.daytimeElapsed);
        Game.Session.gameCanvas.bgLocations.nextBg.bar.Refresh(locationDef,
            Game.Persistence.playerFile.daytimeElapsed,
            Game.Persistence.playerFile.girlPairDefinition,
            Game.Persistence.playerFile.sidesFlipped);

        Game.Session.Location.isTraveling = true;

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
        _sequence = DOTween.Sequence();
        var position = 0f;
        _sequence.Insert(position, Game.Session.gameCanvas.cellphone.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.cellphone.yValues.x, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.header.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.header.yValues.x, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(0f, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.hiddenPosY, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.hiddenPosY, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
        {
            Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
        }, 1f, 0.875f).SetEase(Ease.InOutSine));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(1f, 1.75f).SetEase(Ease.InOutSine));

        position += 1f;

        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.shownSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskShownSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear));

        position += 1.25f;

        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.bgLocations.origPos.y + 1130f, 1.5f).SetEase(Ease.InOutCubic));

        _sequence.OnComplete(() => OnDepartAnimationsComplete(locationDef));

        ModInterface.Events.NotifyLocationDepartSequence(new LocationDepartSequenceArgs() { Sequence = _sequence });

        _sequence.Play();
    }

    private void OnDepartAnimationsComplete(LocationDefinition locationDef)
    {
        Game.Session.gameCanvas.bgLocations.currentBg.art.Refresh(locationDef, Game.Persistence.playerFile.daytimeElapsed);
        Game.Session.gameCanvas.bgLocations.currentBg.bar.Refresh(locationDef, Game.Persistence.playerFile.daytimeElapsed, Game.Persistence.playerFile.girlPairDefinition, Game.Persistence.playerFile.sidesFlipped);
        Game.Session.gameCanvas.bgLocations.rectTransform.anchoredPosition = Game.Session.gameCanvas.bgLocations.origPos;

        Game.Session.Location.bgMusicLink = Game.Manager.Audio.Play(AudioCategory.MUSIC, (Game.Session.Location.bgMusicOverride != null) ? Game.Session.Location.bgMusicOverride : locationDef.bgMusic);

        TweenUtils.KillTween(_sequence);
        _sequence = DOTween.Sequence();
        var position = 1.5f;

        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.bar.frontMaskRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.maskHiddenSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.bar.backgroundRectTransform.DOSizeDelta(Game.Session.gameCanvas.bgLocations.currentBg.bar.hiddenSizeDelta, 0.75f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.shadowsCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.overlaysCanvasGroup.DOFade(0f, 1.75f).SetEase(Ease.InOutSine));

        position += 0.25f;

        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.barsCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.Linear));

        position += 0.25f;

        _sequence.Insert(position, Game.Session.gameCanvas.frameTop.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameTop.origPosition.y, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.frameBottom.rectTransform.DOAnchorPosY(Game.Session.gameCanvas.frameBottom.origPosition.y, 1.25f).SetEase(Ease.InOutCubic));
        _sequence.Insert(position, Game.Session.gameCanvas.bgLocations.currentBg.art.rectTransform.DOAnchorPosY(locationDef.bgYOffset, 1.25f).SetEase(Ease.InOutCubic));

        position += 0.375f;

        _sequence.Insert(position, DOTween.To(() => Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor, delegate (float x)
        {
            Game.Session.gameCanvas.bgLocations.currentBg.art.uiEffect.effectFactor = x;
        }, 0f, 0.875f).SetEase(Ease.InOutSine));

        _sequence.OnComplete(OnArriveComplete);

        ModInterface.Events.NotifyLocationArriveSequence(new LocationArriveSequenceArgs() { Sequence = _sequence });

        _sequence.Play();
    }

    private void OnArriveComplete()
    {
        Complete(this);
    }
}
