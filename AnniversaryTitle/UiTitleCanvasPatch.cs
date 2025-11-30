using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AnniversaryTitle;

[HarmonyPatch(typeof(UiTitleCanvas))]
internal static class TitleCanvasPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start_Pre(UiTitleCanvas __instance)
        => ExpandedUiTitleCanvas.Get(__instance).StartPre();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiTitleCanvas __instance)
        => ExpandedUiTitleCanvas.Get(__instance).OnDestroy();
}

public class ExpandedUiTitleCanvas
{
    private static Dictionary<UiTitleCanvas, ExpandedUiTitleCanvas> _expansions
        = new Dictionary<UiTitleCanvas, ExpandedUiTitleCanvas>();

    public static ExpandedUiTitleCanvas Get(UiTitleCanvas core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiTitleCanvas(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_linkMainTheme = AccessTools.Field(typeof(UiTitleCanvas), "_linkMainTheme");

    private Image _mid;
    private Image _foreground;
    private Image _front;
    private Image _overlay;
    private Image _background;
    private Image _sunrise;

    private Sprite _frontSprite;
    private Sprite _foregroundSprite;
    private Sprite _midSprite;
    private Sprite _backSprite;

    private Sequence _animationSequence;

    private AudioSource _audioSource;

    protected UiTitleCanvas _core;
    private ExpandedUiTitleCanvas(UiTitleCanvas core)
    {
        _core = core;
    }

    public void StartPre()
    {
        var sloppieBoppiePath = Path.Combine(Plugin.AUDIO_DIR, "Sloppie Boppie.wav");
        if (File.Exists(sloppieBoppiePath))
        {
            using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + System.IO.Path.GetFullPath(sloppieBoppiePath), AudioType.WAV))
            {
                request.SendWebRequest();

                //with the way the resource pipeline works atm there's no great way to await this
                while (!request.isDone) System.Threading.Thread.Sleep(1);

                if (request.isNetworkError)
                {
                    ModInterface.Log.LogError($"Error while loading external {nameof(AudioClip)}: {request.error}");
                }
                else
                {
                    _core.musicMainTheme = DownloadHandlerAudioClip.GetContent(request);
                }
            }
        }

        _backSprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "hp_10th_anniversary_art_background.png"), true)).GetSprite();
        _midSprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "hp_10th_anniversary_art_middle.png"), true)).GetSprite();
        _frontSprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "hp_10th_anniversary_art_front.png"), true)).GetSprite();
        _foregroundSprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.IMAGES_DIR, "hp_10th_anniversary_art_foreground.png"), true)).GetSprite();

        _core.StartCoroutine(BuildAnimation());
    }

    private IEnumerator BuildAnimation()
    {
        yield return new WaitForEndOfFrame();
        //ModInterface.ForceUiUpdate ? could be good

        //change bg and fg
        _background = _core.coverArt.transform.GetChild(0).GetComponent<Image>();
        _background.sprite = _backSprite;

        //make logo smaller (do this here so the normal animation isn't interrupted)
        var logoScale = 0.65f;
        var distFromTop = _background.rectTransform.sizeDelta.y - _core.coverArt.logo.localPosition.y;

        _core.coverArt.logo.localPosition = new Vector3(
            _core.coverArt.logo.localPosition.x * logoScale * 0.85f,
            _background.rectTransform.sizeDelta.y - (distFromTop * logoScale * 1.15f), 0);
        _core.coverArt.logo.localScale = new Vector3(logoScale, logoScale, 1);

        _mid = _core.coverArt.transform.GetChild(2).GetComponent<Image>();
        _mid.sprite = _midSprite;

        _mid.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _mid.rectTransform.pivot = new Vector2(0.5f, 0f);

        //add new layers
        var front_go = new GameObject();
        _front = front_go.AddComponent<Image>();
        _front.sprite = _frontSprite;
        _front.rectTransform.SetParent(_core.coverArt.transform, false);

        _front.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _front.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _front.rectTransform.pivot = new Vector2(0.5f, 0f);

        var foreground_go = new GameObject();
        _foreground = foreground_go.AddComponent<Image>();
        _foreground.sprite = _foregroundSprite;
        _foreground.rectTransform.SetParent(_core.coverArt.transform, false);

        _foreground.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _foreground.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _foreground.rectTransform.pivot = new Vector2(0.5f, 0f);

        var overlay_go = new GameObject();
        _overlay = overlay_go.AddComponent<Image>();
        _overlay.rectTransform.SetParent(_core.coverArt.transform, false);

        _overlay.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _overlay.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _overlay.rectTransform.pivot = new Vector2(0.5f, 0f);
        _overlay.color = new Color(1, 1, 1, 0);

        //_core.coverArt.kyu?.DestroyAndKillTweens();
        // _core.coverArt.sunrise?.DestroyAndKillTweens();
        _core.coverArt.altAbia?.Destroy();
        _core.coverArt.kyu.anchoredPosition = new Vector2(3000, 3000);
        _sunrise = _core.coverArt.sunrise.GetComponent<Image>();
        _sunrise.color = new Color(1, 1, 1, 0);

        if (f_linkMainTheme.TryGetValue<AudioLink>(_core, out var audioLink)
            && audioLink.audioSource != null)
        {
            _audioSource = audioLink.audioSource;
            if (Plugin.InitialTitleAnimation)
            {
                _audioSource.time = 14.8f;
            }
            else
            {
                _overlay.color = Color.black;
                _audioSource.time = 0f;
            }
        }

        PlayAnimation();
        _core.StartCoroutine(LoopAnimationWithMusic());
    }

    private void InsertBounces(Sequence sequence, float startingPosition, float intervalLength, float bounceDuration, float bounceScale)
    {
        var timeUp = bounceDuration * (6 / 30f);
        var timeDown = timeUp;
        var timeSettle = bounceDuration - (timeUp + timeDown);
        var loopCount = Mathf.FloorToInt(intervalLength / bounceDuration);

        //mid
        var midSequence = DOTween.Sequence().SetLoops(loopCount);

        midSequence.Append(_mid.rectTransform.DOScaleY(1 + bounceScale, timeUp).SetEase(Ease.InOutCubic));
        midSequence.Append(_mid.rectTransform.DOScaleY(1 - bounceScale, timeDown).SetEase(Ease.OutCubic));
        midSequence.Append(_mid.rectTransform.DOScaleY(1f, timeSettle).SetEase(Ease.OutBack));

        sequence.Insert(startingPosition + timeUp + (timeDown / 5), midSequence);

        //front
        var frontSequence = DOTween.Sequence().SetLoops(loopCount);

        frontSequence.Append(_front.rectTransform.DOScaleY(1 + bounceScale, timeUp).SetEase(Ease.InOutCubic));
        frontSequence.Insert(0f, _front.rectTransform.DOScaleX(1 - bounceScale, timeUp).SetEase(Ease.InOutCubic));

        frontSequence.Append(_front.rectTransform.DOScaleY(1 - bounceScale, timeDown).SetEase(Ease.OutCubic));
        frontSequence.Insert(timeUp, _front.rectTransform.DOScaleX(1 + bounceScale, timeDown).SetEase(Ease.OutCubic));

        frontSequence.Append(_front.rectTransform.DOScaleY(1f, timeSettle).SetEase(Ease.OutBack));
        frontSequence.Insert(timeUp + timeDown, _front.rectTransform.DOScaleX(1f, timeSettle).SetEase(Ease.OutBack));

        sequence.Insert(startingPosition, frontSequence);

        //foreground
        var foregroundSequence = DOTween.Sequence().SetLoops(loopCount);

        foregroundSequence.Append(_foreground.rectTransform.DOScaleX(1f + bounceScale, timeDown + timeSettle).SetEase(Ease.OutElastic));
        foregroundSequence.Append(_foreground.rectTransform.DOScaleX(1f, timeUp).SetEase(Ease.OutSine));

        sequence.Insert(startingPosition + timeUp + (timeDown / 4), foregroundSequence);
    }

    private void PlayAnimation()
    {
        if (_animationSequence != null) TweenUtils.KillTween(_animationSequence);

        _animationSequence = DOTween.Sequence();
        float intervalLength = 0f;
        float sequencePosition = 0f;
        float bounceDuration = 0;
        float bounceScale = 0;
        var fadeTime = 0.75f;

        for (int i = Plugin.InitialTitleAnimation ? 1 : 0; i < 12; i++) //skip intro for first pass
        {
            switch (i)
            {
                case 0://Intro
                    intervalLength = 14.8f;
                    bounceDuration = 0.95f;
                    bounceScale = 0.01f;

                    //remove tints, fade in and out of black
                    _animationSequence.Insert(sequencePosition, _overlay.DOColor(new Color(0, 0, 0, 1), fadeTime));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.2f, 0.2f, 0.0f, 1), fadeTime));
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition + intervalLength - (0.5f * fadeTime), _overlay.DOColor(new Color(0, 0, 0, 0), fadeTime));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

                    break;
                case 1://Theme
                    intervalLength = 23f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //no tint
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

                    break;
                case 2://Dawnwood Park
                    intervalLength = 34f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //green tint
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(0.8f, 1f, 0.8f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(0.95f, 1f, 0.95f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.8f, 1, 0.6f, 1), fadeTime));

                    break;
                case 3://Farmer's Market
                    intervalLength = 43f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //no tint
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

                    break;
                case 4://Shopping Mall
                    intervalLength = 35f;
                    bounceDuration = 0.8f;
                    bounceScale = 0.015f;
                    break;
                case 5://Vinnie's Restaurant
                    intervalLength = 32f;
                    bounceDuration = 0.8f;
                    bounceScale = 0.015f;
                    break;
                case 6://Water Park
                    intervalLength = 38f;
                    bounceDuration = 0.75f;
                    bounceScale = 0.015f;
                    break;
                case 7://In The Bedroom
                    intervalLength = 60f;
                    bounceDuration = 1.7f;
                    bounceScale = 0.025f;

                    //darken bg
                    _animationSequence.Insert(sequencePosition, _overlay.DOColor(new Color(0.1f, 0, 0.2f, 0.3f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.4f, 0.2f, 0.4f, 1), fadeTime));
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(0.9f, 0.7f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 0.9f, 0.95f, 1f), fadeTime).SetEase(Ease.InOutSine));

                    break;
                case 8://Lustie's Nightclub intro
                    intervalLength = 14f;
                    bounceDuration = 4f;
                    bounceScale = 0.005f;

                    //dark overlay
                    _animationSequence.Insert(sequencePosition, _overlay.DOColor(new Color(0.1f, 0, 0.2f, 0.8f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.4f, 0.2f, 0.4f, 1), fadeTime));
                    break;
                case 9://Lustie's Nightclub
                    intervalLength = 34f;
                    bounceDuration = 0.9f;
                    bounceScale = 0.02f;

                    //sunrise
                    _animationSequence.Insert(sequencePosition, _sunrise.DOColor(new Color(1, 1, 1, 0.3f), fadeTime).SetEase(Ease.InOutSine));

                    //tint red
                    _animationSequence.Insert(sequencePosition, _overlay.DOColor(new Color(1, 0, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1f, 0.2f, 0.2f, 1), fadeTime));

                    //rainbow
                    var postRedPos = intervalLength - fadeTime;
                    var rainbowTime = fadeTime * 7;
                    var rainbowLoopCount = Mathf.FloorToInt(postRedPos / rainbowTime);
                    var rainbowTotal = rainbowTime * rainbowLoopCount;
                    var rainbow = DOTween.Sequence().SetLoops(rainbowLoopCount);

                    rainbow.Append(_overlay.DOColor(new Color(1, 0.5f, 0f, 0.2f), fadeTime).SetEase(Ease.InOutSine));//orange
                    rainbow.Append(_overlay.DOColor(new Color(1, 1, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));//yellow
                    rainbow.Append(_overlay.DOColor(new Color(0f, 1, 0f, 0.2f), fadeTime).SetEase(Ease.InOutSine));//green
                    rainbow.Append(_overlay.DOColor(new Color(0, 1, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//L blue
                    rainbow.Append(_overlay.DOColor(new Color(0f, 0f, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//blue
                    rainbow.Append(_overlay.DOColor(new Color(1f, 0f, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//pink
                    rainbow.Append(_overlay.DOColor(new Color(1, 0, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));//red

                    _animationSequence.Insert(sequencePosition + fadeTime, rainbow);
                    _animationSequence.Insert(sequencePosition + fadeTime, _core.coverArt.flasher.DOColor(new Color(1f, 0.5f, 0.2f, 0.5f), fadeTime));

                    var remainingTime = postRedPos - rainbowTotal;
                    if (remainingTime > 0.5f)
                    {
                        _animationSequence.Insert(sequencePosition + fadeTime + rainbowTime, _overlay.DOColor(new Color(1, 1f, 1f, 0f), remainingTime).SetEase(Ease.InOutSine));
                    }

                    break;
                case 10://Reprise
                    intervalLength = 54f;
                    bounceDuration = 0.95f;
                    bounceScale = 0.01f;

                    //turn off sunrise
                    _animationSequence.Insert(sequencePosition, _sunrise.DOColor(new Color(1, 1, 1, 0), fadeTime).SetEase(Ease.InOutSine));

                    //tint pink
                    _animationSequence.Insert(sequencePosition, _overlay.DOColor(new Color(1, 0.95f, 0.95f, 0.05f), 0.75f).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1f, 0.8f, 0.5f, 1f), fadeTime));
                    _animationSequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 0.8f, 0.85f, 1f), 0.75f).SetEase(Ease.InOutSine));
                    _animationSequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 0.9f, 0.95f, 1f), 0.75f).SetEase(Ease.InOutSine));
                    break;
            }

            InsertBounces(_animationSequence, sequencePosition, intervalLength, bounceDuration, bounceScale);
            sequencePosition += intervalLength;
        }

        _animationSequence.Play();

        Plugin.InitialTitleAnimation = false;
    }

    private IEnumerator<object> LoopAnimationWithMusic()
    {
        if (_audioSource != null)
        {
            var lastTime = -999f;
            while (true)//coroutine will be killed on Destroy
            {
                yield return new WaitUntil(() =>
                {
                    if (lastTime > _audioSource.time)//when last time is smaller than current time, return false
                    {
                        return true;
                    }

                    lastTime = _audioSource.time;
                    return false;//when last time is bigger then or equal to current time, return true, when they equal do false
                });

                lastTime = -999f;
                _audioSource.time = 0f;
                if (_audioSource.isActiveAndEnabled) _audioSource.Play();
                PlayAnimation();
            }
        }
    }

    internal void OnDestroy()
    {
        TweenUtils.KillTween(_animationSequence);

        GameObject.Destroy(_frontSprite);
        GameObject.Destroy(_foregroundSprite);
        GameObject.Destroy(_midSprite);
        GameObject.Destroy(_backSprite);
    }
}
