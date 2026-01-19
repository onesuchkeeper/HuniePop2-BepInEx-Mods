using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(EnergyTrailBehavior))]
internal static class EnergyTrailBehaviorPatch
{
    private static readonly FieldInfo f_splashText = AccessTools.Field(typeof(EnergyTrailBehavior), "_splashText");

    [HarmonyPatch(nameof(EnergyTrailBehavior.Init), [typeof(EnergyTrailFormat), typeof(PuzzleReward), typeof(UiPuzzleSlot)])]
    [HarmonyPostfix]
    private static void Init(EnergyTrailBehavior __instance, EnergyTrailFormat format, PuzzleReward reward, UiPuzzleSlot puzzleSlot)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        if (reward.resourceValue != 0
            && reward.resourceType == PuzzleResourceType.BROKEN)
        {
            f_splashText.SetValue(__instance, reward.GetLabelText(false));
        }
    }
}
