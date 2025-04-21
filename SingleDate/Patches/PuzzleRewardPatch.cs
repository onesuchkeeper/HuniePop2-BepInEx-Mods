using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleReward))]
public static class PuzzleRewardPatch
{
    [HarmonyPatch(nameof(PuzzleReward.GetLabelText))]
    [HarmonyPostfix]
    public static void GetLabelText(PuzzleReward __instance, bool shortVersion, ref string __result)
    {
        if (!State.IsSingleDate
            || __instance.zeroedValue
            || __instance.tokenDefinition.resourceType != PuzzleResourceType.BROKEN
            || Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            return;
        }

        __result = $"{__instance.resourceValue}";
        if (!shortVersion)
        {
            __result += " Affection";
        }
    }
}
