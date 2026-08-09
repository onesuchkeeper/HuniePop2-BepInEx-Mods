using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(UiBackgroundArt))]
internal static class UiBackgroundArtPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiBackgroundArt __instance)
        => ExpandedUiBackgroundArt.Get(__instance).Start_Postfix();

    [HarmonyPatch(nameof(UiBackgroundArt.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiBackgroundArt __instance)
    => ExpandedUiBackgroundArt.Get(__instance).Refresh_Postfix();
}

[HarmonyPatch(typeof(UiBackgroundBlur))]
internal static class UiBackgroundBlurPatch
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

[Expansion(typeof(UiBackgroundArt))]
public partial class ExpandedUiBackgroundArt
{
    private Vector2 _bgRectSizeDelta;

    internal void Start_Postfix()
    {
        _core.image.type = Image.Type.Simple;
        _core.image.useSpriteMesh = true;
        _core.image.preserveAspect = false;
        _bgRectSizeDelta = _core.image.rectTransform.sizeDelta;
    }

    internal void Refresh_Postfix()
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
