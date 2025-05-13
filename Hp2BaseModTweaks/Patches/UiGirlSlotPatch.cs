using HarmonyLib;
using UnityEngine;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiGirlSlot))]
public static class UiGirlSlotPatch
{

    [HarmonyPatch(nameof(UiGirlSlot.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiGirlSlot __instance)
    {
        if (__instance.portraitImage.sprite != null)
        {
            var ratio = __instance.portraitImage.sprite.rect.width / __instance.portraitImage.sprite.rect.height;
            var oldSize = __instance.portraitImage.rectTransform.sizeDelta;
            __instance.portraitImage.rectTransform.sizeDelta = new Vector2(oldSize.y * ratio, oldSize.y);
        }
    }
}