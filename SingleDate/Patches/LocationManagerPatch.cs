using HarmonyLib;
using Hp2BaseMod.Utility;

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

    [HarmonyPatch(nameof(LocationManager.Depart))]
    [HarmonyPrefix]
    public static void Depart(LocationManager __instance, LocationDefinition locationDef, GirlPairDefinition girlPairDef, ref bool sidesFlipped)
    {
        if (State.IsSingle(girlPairDef))
        {
            sidesFlipped = false;
        }
    }
}
