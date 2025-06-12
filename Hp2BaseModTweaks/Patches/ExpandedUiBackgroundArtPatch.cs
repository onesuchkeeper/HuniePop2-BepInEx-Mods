using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseModTweaks;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(UiBackgroundArt))]
public static class UiBackgroundArtPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiBackgroundArt __instance)
        => ExpandedUiBackgroundArt.Get(__instance).Start();

    [HarmonyPatch(nameof(UiBackgroundArt.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiBackgroundArt __instance)
    => ExpandedUiBackgroundArt.Get(__instance).Refresh();
}

[HarmonyPatch(typeof(UiBackgroundBlur))]
public static class UiBackgroundBlurPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiBackgroundBlur __instance)
    {
        __instance.image.useSpriteMesh = true;
    }
}

public class ExpandedUiBackgroundArt
{
    private static Dictionary<UiBackgroundArt, ExpandedUiBackgroundArt> _expansions
        = new Dictionary<UiBackgroundArt, ExpandedUiBackgroundArt>();

    public static ExpandedUiBackgroundArt Get(UiBackgroundArt core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiBackgroundArt(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private Image _bgImage;
    protected UiBackgroundArt _core;
    private ExpandedUiBackgroundArt(UiBackgroundArt core)
    {
        _core = core;
    }

    public void Start()
    {
        _core.image.useSpriteMesh = true;
        _core.image.preserveAspect = true;

        var bg_go = new GameObject();
        _bgImage = bg_go.AddComponent<Image>();
        _bgImage.useSpriteMesh = true;

        _bgImage.rectTransform.SetParent(_core.image.rectTransform.parent);
        _bgImage.rectTransform.SetAsFirstSibling();
        _bgImage.rectTransform.sizeDelta = _core.image.rectTransform.sizeDelta;
        _bgImage.rectTransform.localPosition = _core.image.rectTransform.localPosition;
        _bgImage.sprite = _core.image.sprite;

        ModInterface.Events.LocationArriveSequence += On_LocationArriveSequence;
        ModInterface.Events.LocationDepartSequence += On_LocationDepartSequence;
    }

    private void On_LocationDepartSequence(LocationDepartSequenceArgs args)
    {
        args.Sequence ??= DOTween.Sequence();

        Saturation = 1;

        args.Sequence.Insert(0.75f, DOTween.To(
                () => Saturation,
                x => Saturation = x,
                0f,
                0.875f)
            .SetEase(Ease.InOutSine));
    }

    private void On_LocationArriveSequence(LocationArriveSequenceArgs args)
    {
        args.Sequence ??= DOTween.Sequence();

        Saturation = 0;

        args.Sequence.Insert(2.375f, DOTween.To(
                () => Saturation,
                x => Saturation = x,
                1f,
                0.875f)
            .SetEase(Ease.InOutSine));
    }

    private float Saturation
    {
        get => x_saturation;
        set
        {
            //_bgImage.material.SetFloat("_Saturation", value);
            x_saturation = value;
        }
    }
    private float x_saturation = 0;

    internal void Refresh()
    {
        _bgImage.sprite = _core.image.sprite;

        // if (_bgImage.sprite != null)
        // {
        //     var ratio = (_bgImage.sprite.rect.size.x / _bgImage.sprite.rect.size.y)
        //         / (_bgImage.rectTransform.sizeDelta.x / _bgImage.rectTransform.sizeDelta.y);

        //     _bgImage.material.SetFloat("_AspectRatio", ratio);
        // }
    }
}
