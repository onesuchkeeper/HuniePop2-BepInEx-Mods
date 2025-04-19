using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
public static class CutsceneStepSpecialPostRewardsPatch
{
    private static FieldInfo _postRewards = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_postRewards");

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(CutsceneStepSpecialPostRewards __instance)
    {
        if (!State.IsLocationPairSingle())
        {
            return;
        }

        if (!(_postRewards.GetValue(__instance) is List<PuzzlePostReward> postRewards))
        {
            ModInterface.Log.LogWarning($"Failed to get {nameof(CutsceneStepSpecialPostRewards)} _postRewards");
            return;
        }

        //alt girl is girl on right
        // for single dates put all rewards there
        foreach (var reward in postRewards)
        {
            reward.altGirl = true;
        }
    }
}
