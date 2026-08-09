using HarmonyLib;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(ItemSlotBehavior))]
internal static class ItemSlotBehaviorPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(ItemSlotBehavior __instance)
    {
        __instance.itemIcon.useSpriteMesh = true;
    }
}