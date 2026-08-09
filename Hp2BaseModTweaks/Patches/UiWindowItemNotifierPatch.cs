using HarmonyLib;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiWindowItemNotifier))]
internal static class UiWindowItemNotifierPatch
{
    [HarmonyPatch(nameof(UiWindowItemNotifier.Init))]
    [HarmonyPostfix]
    public static void Populate(UiWindowItemNotifier __instance)
    {
        __instance.itemIcon.useSpriteMesh = true;
    }
}