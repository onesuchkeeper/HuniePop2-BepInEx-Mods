using System.Collections.Generic;
using HarmonyLib;
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
        __instance.image.type = Image.Type.Simple;
        __instance.image.useSpriteMesh = true;
        __instance.image.preserveAspect = false;
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

    private Vector2 _bgRectSizeDelta;
    private UiBackgroundArt _core;
    private ExpandedUiBackgroundArt(UiBackgroundArt core)
    {
        _core = core;
    }

    public void Start()
    {
        _core.image.type = Image.Type.Simple;
        _core.image.useSpriteMesh = true;
        _core.image.preserveAspect = false;
        _bgRectSizeDelta = _core.image.rectTransform.sizeDelta;
    }

    internal void Refresh()
    {
        var image = _core.image;
        var sprite = image.sprite;
        if (sprite == null) return;

        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;

        float rectWidth = _bgRectSizeDelta.x;
        float rectHeight = _bgRectSizeDelta.y;

        float spriteRatio = spriteWidth / spriteHeight;
        float rectRatio = rectWidth / rectHeight;

        Vector2 size;

        if (rectRatio > spriteRatio)
        {
            size.x = rectWidth;
            size.y = rectWidth / spriteRatio;
        }
        else
        {
            size.y = rectHeight;
            size.x = rectHeight * spriteRatio;
        }

        image.rectTransform.sizeDelta = size;
    }
}
