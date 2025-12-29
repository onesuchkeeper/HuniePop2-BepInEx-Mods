using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(CursorBehavior))]
public static class CursorBehaviourPatch
{

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake(CursorBehavior __instance)
    {
        __instance.image.useSpriteMesh = true;
    }
}