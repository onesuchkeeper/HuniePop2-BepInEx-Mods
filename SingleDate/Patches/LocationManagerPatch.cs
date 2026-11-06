using HarmonyLib;

namespace SingleDate;

/// <summary>Builds the single date UI prefabs once the base game's location manager exists, since they are copies of its objects.</summary>
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
