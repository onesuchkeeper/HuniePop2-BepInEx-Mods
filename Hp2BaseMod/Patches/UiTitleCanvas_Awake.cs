// Hp2BaseMod 2026, By OneSuchKeeper

using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Simple hook to notify when the title canvas is ready.
/// </summary>
[HarmonyPatch(typeof(UiTitleCanvas))]
internal static class TitleCanvasPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix] 
    private static void Start_Postfix(UiTitleCanvas __instance) =>  ModInterface.Events.NotifyTitleCanvasReady(__instance);
}