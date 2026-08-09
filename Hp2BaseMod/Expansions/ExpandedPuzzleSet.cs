using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(PuzzleSet))]
[Deprecates("GetMatchRewards", $"Use {nameof(ExpandedPuzzleSet)}.{nameof(ExpandedPuzzleSet.GetRewards)} instead")]
public partial class ExpandedPuzzleSet
{
    [HarmonyPatch(typeof(PuzzleSet))]
    private static class Patch
    {
        /// <summary>
        /// Replaces GetMatchRewards entirely. Returns false to skip the original.
        /// All reward calculation now runs through the scripted pipeline.
        /// </summary>
        [HarmonyPatch("GetMatchRewards")]
        [HarmonyPrefix]
        private static bool GetMatchRewards(PuzzleSet __instance, PuzzleMatch match, bool altGirl, ref Dictionary<UiPuzzleSlot, PuzzleReward> __result)
            => ExpandedPuzzleSet.Get(__instance).GetMatchRewards_Prefix(match, altGirl, ref __result);
    }

    public Dictionary<UiPuzzleSlot, ISlotReward> GetRewards()
    {
        var result = new Dictionary<UiPuzzleSlot, ISlotReward>();

        var altGirlFocused = Game.Session.Puzzle.puzzleStatus.altGirlFocused;

        foreach(var entry in _core.matches.SelectMany(x => GetRewards(x, altGirlFocused)))
        {
            if (!result.TryGetValue(entry.Slot, out var slotReward))
            {
                slotReward = new SlotPuzzleReward();
                result[entry.Slot] = slotReward;
            }

            slotReward.Rewards.Add(entry.Reward);
        }

        return result;
    }

    private IEnumerable<(UiPuzzleSlot Slot, IPuzzleReward Reward)> GetRewards(PuzzleMatch match, bool altGirl)
    {
        var uiPuzzleGrid = ExpandedUiPuzzleGrid.Get();
        var status = uiPuzzleGrid._status;
        var matchModifier = Game.Session.Ailment.Trigger(match);
        if (matchModifier.tokenDefinition != null)
        {
            match.tokenDefinition = matchModifier.tokenDefinition;
        }

        if (matchModifier.absorb)
        {
            altGirl = matchModifier.absorbAltGirl;
        }

        var statusGirl = status.GetStatusGirl(altGirl);
        Game.Persistence.playerFile.GetPlayerFileGirl(statusGirl.girlDefinition);

        var orderedSlots = match.slots
            .OrderBy(slot => slot.row + slot.col)
            .ThenBy(_ => UnityEngine.Random.Range(0f, 1f))
            .ToList();

        var sortedSlots = new List<UiPuzzleSlot>();
        while (orderedSlots.Count > 0)
        {
            float center = (orderedSlots.Count - 1) * 0.5f;
            int index = Mathf.Clamp(
                MathUtils.RandomBool() 
                    ? Mathf.FloorToInt(center) 
                    : Mathf.CeilToInt(center), 
                0, 
                orderedSlots.Count - 1);

            sortedSlots.Add(orderedSlots[index]);
            orderedSlots.RemoveAt(index);
        }

        var ailmentManager = Game.Session.Ailment.GetExpansion();
        int totalRewardAmount;
        var tokenExp = match.tokenDefinition.GetExpansion();
        if (!Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            totalRewardAmount = tokenExp.PuzzleResource.CalculateMatchReward(
                uiPuzzleGrid,
                ailmentManager, 
                this, 
                Game.Persistence.playerFile, 
                match, 
                matchModifier, 
                statusGirl, 
                sortedSlots);

            if (matchModifier.pointsOp)
            {
                totalRewardAmount = Mathf.RoundToInt(MathUtils.CombineValues(matchModifier.pointsOperation, totalRewardAmount, matchModifier.pointsFactor));
            }

            if (matchModifier.pointsOp2)
            {
                totalRewardAmount = Mathf.RoundToInt(MathUtils.CombineValues(matchModifier.pointsOperation2, totalRewardAmount, matchModifier.pointsFactor2));
            }

            //it'd be best to move this to its own ailment, but I'll leave it here for now
            if (totalRewardAmount >= 5000 && (
                tokenExp.Id == PuzzleResourceId.AffectionFlirtation
                ||tokenExp.Id == PuzzleResourceId.AffectionTalent
                ||tokenExp.Id == PuzzleResourceId.AffectionRomance
                ||tokenExp.Id == PuzzleResourceId.AffectionSexuality))
            {
                Game.Manager.Platform.UnlockAchievement("smooth_move");
            }
        }
        else
        {
            totalRewardAmount = sortedSlots.Count
                * Mathf.Max(sortedSlots.Count - 2, 1)
                * 10;
        }

        var negativeReward = totalRewardAmount < 0;
        totalRewardAmount = Mathf.Abs(totalRewardAmount);
        var absoluteRewardAmount = totalRewardAmount;
        var result = new List<(UiPuzzleSlot, IPuzzleReward)>();

        for (int i = 0; i < sortedSlots.Count; i++)
        {
            var portion = tokenExp.PuzzleResource.CalculateRewardPortion(totalRewardAmount, sortedSlots.Count, i);
            totalRewardAmount -= portion;
            var puzzleReward = new TokenPuzzleReward(match.tokenDefinition.GetExpansion(), (!negativeReward) ? portion : (-portion), statusGirl)
            {
                ZeroedValue = absoluteRewardAmount == 0
            };

            //I don't think this is needed, only affection tokens spawn durring bonus rounds anyways
            //If we need it just create a special 'PleasureReward' instead
            // if (Game.Session.Puzzle.puzzleStatus.bonusRound)
            // {
            //     puzzleReward.resourceType = PuzzleResourceType.AFFECTION;
            // }

            if (!Game.Session.Puzzle.puzzleStatus.bonusRound 
                && !Game.Session.Puzzle.puzzleStatus.IsTutorial() 
                && !match.flatMatch && i == 0)
            {
                if (matchModifier.replaceDefinition != null && matchModifier.replacePriority)
                {
                    puzzleReward.ReplaceDefinition = matchModifier.replaceDefinition;
                }

                if (puzzleReward.ReplaceDefinition == null)
                {
                    tokenExp.PuzzleResource.HandleReplaceDefinition(absoluteRewardAmount, match, matchModifier, puzzleReward, sortedSlots);
                }
            }

            result.Add((sortedSlots[i],puzzleReward));
        }

        var postArgs = new AilmentTriggerArgs.PostMatchReward(uiPuzzleGrid, statusGirl, result);
        ailmentManager.OnPostMatchReward(postArgs);
        return result;
    }

    //OLD
    private bool GetMatchRewards_Prefix(PuzzleMatch match, bool altGirl, ref Dictionary<UiPuzzleSlot, PuzzleReward> __result)
    {
        ModInterface.Log.Error($"This method has been depreciated, use {nameof(ExpandedPuzzleSet)}.{nameof(ExpandedPuzzleSet.GetRewards)} instead");
        __result = new();
        return false;
    }
}