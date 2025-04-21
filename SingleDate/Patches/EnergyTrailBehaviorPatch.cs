using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(EnergyTrailBehavior))]
public static class EnergyTrailBehaviorPatch
{
    private static FieldInfo _splashText = AccessTools.Field(typeof(EnergyTrailBehavior), "_splashText");

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
            _splashText.SetValue(__instance, reward.GetLabelText(false));
        }
    }
}
