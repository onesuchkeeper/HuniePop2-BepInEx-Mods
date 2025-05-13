using HarmonyLib;
using UnityEngine;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiStatusPortrait))]
public static class UiStatusPortraitPatch
{

    [HarmonyPatch(nameof(UiStatusPortrait.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiStatusPortrait __instance)
    {
        if (__instance.portraitImage.sprite != null)
        {
            var ratio = __instance.portraitImage.sprite.rect.width / __instance.portraitImage.sprite.rect.height;
            var oldSize = __instance.portraitImage.rectTransform.sizeDelta;
            __instance.portraitImage.rectTransform.sizeDelta = new Vector2(oldSize.y * ratio, oldSize.y);
        }
    }
}