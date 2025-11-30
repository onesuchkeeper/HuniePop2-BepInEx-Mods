using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace SingleDate;

[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
internal static class CutsceneStepSpecialPostRewardsPatch
{
    private static FieldInfo f_postRewards = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_postRewards");

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(CutsceneStepSpecialPostRewards __instance)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        // alt girl is girl on right
        // for single dates put all rewards there
        foreach (var reward in f_postRewards.GetValue<List<PuzzlePostReward>>(__instance))
        {
            reward.altGirl = true;
        }
    }
}
