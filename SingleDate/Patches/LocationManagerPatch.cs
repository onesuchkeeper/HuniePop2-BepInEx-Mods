using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake(LocationManager __instance)
    {
        UiPrefabs.InitActionBubbles(__instance.actionBubblesWindow);
        UiPrefabs.InitCutsceneMeeting(__instance.cutsceneMeeting);
    }
}
