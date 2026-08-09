using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiPuzzleStaminaHint))]
internal static class UiPuzzleStaminaHintPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleStaminaHint __instance)
        => ExpandedUiPuzzleStaminaHint.Get(__instance).Start_Postfix();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleStaminaHint __instance)
        => ExpandedUiPuzzleStaminaHint.Destroy(__instance);
}

[Expansion(typeof(UiPuzzleStaminaHint))]
public partial class ExpandedUiPuzzleStaminaHint
{
    internal void Start_Postfix()
    {
        ExpandedUiPuzzleGrid.Get().MoveCompleteEvent += OnShouldCheck;
    }

    private void OnDestroy()
    {
        ExpandedUiPuzzleGrid.Get().MoveCompleteEvent -= OnShouldCheck;
    }
}