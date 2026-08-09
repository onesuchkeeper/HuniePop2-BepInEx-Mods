using HarmonyLib;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(CursorBehavior))]
internal static class CursorBehaviourPatch
{

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake(CursorBehavior __instance)
    {
        __instance.image.useSpriteMesh = true;
    }
}