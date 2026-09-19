using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiTitleCanvas))]
internal static class TitleCanvasPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix] 
    private static void Start_Postfix(UiTitleCanvas __instance) =>  ModInterface.Events.NotifyTitleCanvasReady(__instance);
}