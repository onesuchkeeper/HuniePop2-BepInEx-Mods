using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleSet))]
public static class PuzzleSetPatch
{
    private static readonly FieldInfo _affection = AccessTools.Field(typeof(PuzzleStatus), "_affection");

    [HarmonyPatch("GetMatchRewards")]
    [HarmonyPostfix]
    public static void GetMatchRewards(PuzzleSet __instance, PuzzleMatch match, bool altGirl, ref Dictionary<UiPuzzleSlot, PuzzleReward> __result)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        var brokenRewards = __result.Values.Where(x => x.resourceType == PuzzleResourceType.BROKEN);
        var brokenCount = brokenRewards.Count();

        if (brokenCount == 0)
        {
            return;
        }

        //hp1's broken tokens take a percentage of the current affection, multiply by (number of tokens), then divide by (number of tokens - 1), with min value of 1
        //so I'll just ignore the mult and divide entirely. The -1 looks like it may be a mistake? Like they were confusing it with max index? seems odd
        var allotment = -Mathf.Max(1,
            Mathf.FloorToInt(_affection.GetValue<int>(Game.Session.Puzzle.puzzleStatus)
                * State.GetBrokenMult()));

        foreach (var reward in brokenRewards)
        {
            reward.resourceValue = allotment;
        }
    }

    [HarmonyPatch(nameof(PuzzleSet.GetStaminaCost))]
    [HarmonyPostfix]
    public static void GetStaminaCost(PuzzleSet __instance, bool raw, bool forceFull, ref int __result)
    {
        if (!State.IsSingleDate || raw)
        {
            return;
        }

        __result = 0;
    }
}
