using System;
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

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start_Post(UiTitleCanvas __instance)
        => ExpandedUiTitleCanvas.Get(__instance).StartPost();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiTitleCanvas __instance)
        => ExpandedUiTitleCanvas.Get(__instance).OnDestroy();

    [HarmonyPatch("OnInitialAnimationComplete")]
    [HarmonyPostfix]
    public static void OnInitialAnimationComplete(UiTitleCanvas __instance)
        => ExpandedUiTitleCanvas.Get(__instance).OnInitialAnimationComplete();
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

    private static readonly FieldInfo _linkMainTheme = AccessTools.Field(typeof(UiTitleCanvas), "_linkMainTheme");

    private Image _mid;
    private Image _foreground;
    private Image _front;
    private Image _overlay;
    private Image _background;

    private Sequence _animationSequence;

    private AudioSource _audioSource;

    protected UiTitleCanvas _core;
    private ExpandedUiTitleCanvas(UiTitleCanvas core)
    {
        _core = core;
    }

    public void StartPre()
    {
        var sloppieBoppiePath = Path.Combine(Plugin.AudioDir, "Sloppie Boppie.wav");
        if (File.Exists(sloppieBoppiePath))
        {
            using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + System.IO.Path.GetFullPath(sloppieBoppiePath), AudioType.WAV))
            {
                request.SendWebRequest();

                //with the way the resource pipeline works atm there's no great way to await this
                while (!request.isDone) { }

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
    }

    public void StartPost()
    {
        //change bg and fg
        _background = _core.coverArt.transform.GetChild(0).GetComponent<Image>();
        _background.sprite
            = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "hp_10th_anniversary_art_background.png"))).GetSprite();

        _mid = _core.coverArt.transform.GetChild(2).GetComponent<Image>();
        _mid.sprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "hp_10th_anniversary_art_middle.png"))).GetSprite();

        _mid.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _mid.rectTransform.pivot = new Vector2(0.5f, 0f);

        //make logo smaller
        var logoScale = 0.65f;

        var distFromTop = _background.rectTransform.sizeDelta.y - _core.coverArt.logo.localPosition.y;

        _core.coverArt.logo.localPosition = new Vector3(
            _core.coverArt.logo.localPosition.x * logoScale * 0.85f,
            _background.rectTransform.sizeDelta.y - (distFromTop * logoScale * 1.15f), 0);
        _core.coverArt.logo.localScale = new Vector3(logoScale, logoScale, 1);

        //add new layers
        var front_go = new GameObject();
        _front = front_go.AddComponent<Image>();
        _front.sprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "hp_10th_anniversary_art_front.png"))).GetSprite();
        _front.rectTransform.SetParent(_core.coverArt.transform);

        _front.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _front.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _front.rectTransform.pivot = new Vector2(0.5f, 0f);

        var foreground_go = new GameObject();
        _foreground = foreground_go.AddComponent<Image>();
        _foreground.sprite = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImagesDir, "hp_10th_anniversary_art_foreground.png"))).GetSprite();
        _foreground.rectTransform.SetParent(_core.coverArt.transform);

        _foreground.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _foreground.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _foreground.rectTransform.pivot = new Vector2(0.5f, 0f);

        var overlay_go = new GameObject();
        _overlay = overlay_go.AddComponent<Image>();
        _overlay.rectTransform.SetParent(_core.coverArt.transform);

        _overlay.rectTransform.sizeDelta = _background.rectTransform.sizeDelta;
        _overlay.rectTransform.localPosition = new Vector3(_background.rectTransform.sizeDelta.x / 2, 0, 0);
        _overlay.rectTransform.pivot = new Vector2(0.5f, 0f);
        _overlay.color = new Color(1, 1, 1, 0);

        //the tweens hate me when I destroy them too early, and they don't let me stop them early, so move these off screen and delete them later I guess
        _core.coverArt.kyu.position = new Vector3(3000, 3000);
        _core.coverArt.sunrise.position = new Vector3(3000, 3000);
        _core.coverArt.altAbia.rectTransform.position = new Vector3(3000, 3000);

        _audioSource = _linkMainTheme.GetValue<AudioLink>(_core).audioSource;

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

    public void PlayAnimation()
    {
        var sequence = DOTween.Sequence();
        float intervalLength = 0f;
        float sequencePosition = 0f;
        float bounceDuration = 0;
        float bounceScale = 0;
        var fadeTime = 0.75f;

        for (int i = Plugin.InitialTitleAnimation ? 1 : 0; i < 12; i++)//skip intro for first pass
        {
            switch (i)
            {
                case 0://Intro
                    intervalLength = 14.8f;
                    bounceDuration = 0.95f;
                    bounceScale = 0.01f;

                    //remove tints, fade in and out of black
                    sequence.Insert(sequencePosition, _overlay.DOColor(new Color(0, 0, 0, 1), fadeTime));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.2f, 0.2f, 0.0f, 1), fadeTime));
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition + intervalLength - (0.5f * fadeTime), _overlay.DOColor(new Color(0, 0, 0, 0), fadeTime));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

                    break;
                case 1://Theme
                    intervalLength = 23f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //no tint
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

                    break;
                case 2://Dawnwood Park
                    intervalLength = 34f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //green tint
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(0.8f, 1f, 0.8f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(0.95f, 1f, 0.95f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.8f, 1, 0.6f, 1), fadeTime));

                    break;
                case 3://Farmer's Market
                    intervalLength = 43f;
                    bounceDuration = 0.89f;
                    bounceScale = 0.015f;

                    //no tint
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 1f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1, 1, 1, 1), fadeTime));

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
                    sequence.Insert(sequencePosition, _overlay.DOColor(new Color(0.1f, 0, 0.2f, 0.3f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.4f, 0.2f, 0.4f, 1), fadeTime));
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(0.9f, 0.7f, 1f, 1f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 0.9f, 0.95f, 1f), fadeTime).SetEase(Ease.InOutSine));

                    break;
                case 8://Lustie's Nightclub intro
                    intervalLength = 14f;
                    bounceDuration = 4f;
                    bounceScale = 0.005f;

                    //dark overlay
                    sequence.Insert(sequencePosition, _overlay.DOColor(new Color(0.1f, 0, 0.2f, 0.8f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(0.4f, 0.2f, 0.4f, 1), fadeTime));
                    break;
                case 9://Lustie's Nightclub
                    intervalLength = 34f;
                    bounceDuration = 0.9f;
                    bounceScale = 0.02f;

                    //tint red
                    sequence.Insert(sequencePosition, _overlay.DOColor(new Color(1, 0, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1f, 0.2f, 0.2f, 1), fadeTime));

                    //rainbow
                    var postRedPos = intervalLength - fadeTime;
                    var rainbowTime = fadeTime * 7;
                    var rainbowLoopCount = Mathf.FloorToInt(postRedPos / rainbowTime);
                    var rainbowTotal = rainbowTime * rainbowLoopCount;
                    var raindow = DOTween.Sequence().SetLoops(rainbowLoopCount);

                    raindow.Append(_overlay.DOColor(new Color(1, 0.5f, 0f, 0.2f), fadeTime).SetEase(Ease.InOutSine));//orange
                    raindow.Append(_overlay.DOColor(new Color(1, 1, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));//yellow
                    raindow.Append(_overlay.DOColor(new Color(0f, 1, 0f, 0.2f), fadeTime).SetEase(Ease.InOutSine));//green
                    raindow.Append(_overlay.DOColor(new Color(0, 1, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//L blue
                    raindow.Append(_overlay.DOColor(new Color(0f, 0f, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//blue
                    raindow.Append(_overlay.DOColor(new Color(1f, 0f, 1, 0.2f), fadeTime).SetEase(Ease.InOutSine));//pink
                    raindow.Append(_overlay.DOColor(new Color(1, 0, 0, 0.2f), fadeTime).SetEase(Ease.InOutSine));//red

                    sequence.Insert(sequencePosition + fadeTime, raindow);
                    sequence.Insert(sequencePosition + fadeTime, _core.coverArt.flasher.DOColor(new Color(1f, 0.5f, 0.2f, 0.5f), fadeTime));

                    var remainingTime = postRedPos - rainbowTotal;
                    if (remainingTime > 0.5f)
                    {
                        sequence.Insert(sequencePosition + fadeTime + rainbowTime, _overlay.DOColor(new Color(1, 1f, 1f, 0f), remainingTime).SetEase(Ease.InOutSine));
                    }

                    break;
                case 10://Reprise
                    intervalLength = 54f;
                    bounceDuration = 0.95f;
                    bounceScale = 0.01f;

                    //tint pink
                    sequence.Insert(sequencePosition, _overlay.DOColor(new Color(1, 0.95f, 0.95f, 0.05f), 0.75f).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _core.coverArt.flasher.DOColor(new Color(1f, 0.8f, 0.5f, 1f), fadeTime));
                    sequence.Insert(sequencePosition, _background.DOColor(new Color(1f, 0.8f, 0.85f, 1f), 0.75f).SetEase(Ease.InOutSine));
                    sequence.Insert(sequencePosition, _mid.DOColor(new Color(1f, 0.9f, 0.95f, 1f), 0.75f).SetEase(Ease.InOutSine));
                    break;
            }

            InsertBounces(sequence, sequencePosition, intervalLength, bounceDuration, bounceScale);
            sequencePosition += intervalLength;
        }

        sequence.Play();

        _animationSequence = sequence;
        Plugin.InitialTitleAnimation = false;
    }

    internal void OnInitialAnimationComplete()
    {
        PlayAnimation();

        _core.StartCoroutine(WaitForMusicToFinish());
    }

    private IEnumerator<object> WaitForMusicToFinish()
    {
        var lastTime = -999f;
        yield return new WaitUntil(() =>
        {
            if (lastTime > _audioSource.time)//when last time is smaller than current time, return false
            {
                return true;
            }

            lastTime = _audioSource.time;
            return false;//when last time is bigger then or equal to current time, return true, when they equal do false
            return _audioSource.time >= _audioSource.clip.length; //return true when time is bigger than the song's lenth
        });

        TweenUtils.KillTween(_animationSequence);

        _audioSource.time = 0f;
        _audioSource.Play();
        PlayAnimation();

        _core.StartCoroutine(WaitForMusicToFinish());
    }

    internal void OnDestroy()
    {
        TweenUtils.KillTween(_animationSequence);
    }
}
