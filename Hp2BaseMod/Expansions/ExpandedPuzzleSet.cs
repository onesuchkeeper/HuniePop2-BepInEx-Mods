using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod
{
    [Expansion(typeof(PuzzleSet))]
    [Deprecates("GetMatchRewards", $"Use {nameof(ExpandedPuzzleSet)}.{nameof(ExpandedPuzzleSet.GetRewards)} instead")]
    [Deprecates(nameof(PuzzleSet.HasMatchWithResourceType), $"Use {nameof(ExpandedPuzzleSet)}.{nameof(ExpandedPuzzleSet.HasMatchWithResource)} instead")]
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

            [HarmonyPatch(nameof(PuzzleSet.GetStaminaCost))]
            [HarmonyPrefix]
            private static bool GetStaminaCost(PuzzleSet __instance, bool raw, bool forceFull, ref int __result)
                => ExpandedPuzzleSet.Get(__instance).GetStaminaCost_Prefix(raw, forceFull, ref __result);

            [HarmonyPatch(nameof(PuzzleSet.GetMovesCost))]
            [HarmonyPrefix]
            private static bool GetMovesCost(PuzzleSet __instance, ref int __result)
                => ExpandedPuzzleSet.Get(__instance).GetMovesCost_Prefix(ref __result);

            [HarmonyPatch(nameof(PuzzleSet.HasMatchWithResourceType))]
            [HarmonyPrefix]
            private static bool HasMatchWithResourceType(PuzzleSet __instance, PuzzleResourceType resourceType, bool inverse, ref bool __result)
                => ExpandedPuzzleSet.Get(__instance).HasMatchWithResourceType_Prefix(resourceType, inverse, ref __result);
        }

        public Dictionary<UiPuzzleSlot, ISlotReward> GetRewards()
        {
            var result = new Dictionary<UiPuzzleSlot, ISlotReward>();

            var altGirlFocused = Game.Session.Puzzle.puzzleStatus.altGirlFocused;

            foreach (var entry in _core.matches.SelectMany(x => GetRewards(x, altGirlFocused)))
            {
                if (!result.TryGetValue(entry.Slot, out var slotReward))
                {
                    slotReward = new SlotPuzzleReward();
                    result[entry.Slot] = slotReward;
                }

                slotReward.Rewards.Add(entry.Reward);

                if (entry.Reward.ReplaceDefinition != null)
                {
                    slotReward.ReplaceDefinition = entry.Reward.ReplaceDefinition;
                }

                if (entry.Reward.ReplaceUpgraded)
                {
                    slotReward.ReplaceUpgraded = true;
                }
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

                if (totalRewardAmount >= 5000 && tokenExp.PuzzleResource is PuzzleResourceAffection)
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

                result.Add((sortedSlots[i], puzzleReward));
            }

            var postArgs = new AilmentTriggerArgs.PostMatchReward(uiPuzzleGrid, statusGirl, result);
            ailmentManager.OnPostMatchReward(postArgs);
            return result;
        }

        private bool GetMatchRewards_Prefix(PuzzleMatch match, bool altGirl, ref Dictionary<UiPuzzleSlot, PuzzleReward> __result)
        {
            ModInterface.Log.Error($"This method has been depreciated, use {nameof(ExpandedPuzzleSet)}.{nameof(ExpandedPuzzleSet.GetRewards)} instead");
            __result = new();
            return false;
        }

        public bool HasMatchWithResource(IPuzzleResource resource, bool inverse = false)
        {
            return HasMatchWithResource(r => r == resource, inverse);
        }

        public bool HasMatchWithResource(RelativeId resourceId, bool inverse = false)
        {
            return HasMatchWithResource(r => r.Id == resourceId, inverse);
        }

        public bool HasMatchWithResource(Func<IPuzzleResource, bool> predicate, bool inverse = false)
        {
            if (predicate == null) return inverse;

            for (var i = 0; i < _core.matches.Count; i++)
            {
                var tokenExp = _core.matches[i].tokenDefinition.GetExpansion();
                if (tokenExp.PuzzleResource != null && predicate(tokenExp.PuzzleResource))
                {
                    return !inverse;
                }
            }
            return inverse;
        }

        private bool HasMatchWithResourceType_Prefix(PuzzleResourceType resourceType, bool inverse, ref bool __result)
        {
            var targetTokenDef = Game.Data.Tokens.GetByResourceType(resourceType);
            var targetResource = targetTokenDef?.GetExpansion().PuzzleResource;

            __result = HasMatchWithResource(r => r == targetResource, inverse);
            return false;
        }

        private bool GetStaminaCost_Prefix(bool raw, bool forceFull, ref int __result)
        {
            var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
            if (puzzleStatus.bonusRound)
            {
                __result = 0;
                return false;
            }

            if (!raw)
            {
                foreach (var match in _core.matches)
                {
                    var tokenExp = match.tokenDefinition.GetExpansion();
                    if (tokenExp.BypassesStaminaCost(puzzleStatus))
                    {
                        __result = 0;
                        return false;
                    }
                }
            }

            if (!forceFull && puzzleStatus.girlStatusFocused.flatStaminaCost > 0)
            {
                __result = 1;
                return false;
            }

            var cost = 0;
            foreach (var match in _core.matches)
            {
                var tokenExp = match.tokenDefinition.GetExpansion();
                cost += tokenExp.GetStaminaCost(match);
            }

            __result = Mathf.Clamp(cost, 1, 6);
            return false;
        }

        private bool GetMovesCost_Prefix(ref int __result)
        {
            var puzzleStatus = Game.Session.Puzzle.puzzleStatus;

            foreach (var match in _core.matches)
            {
                var tokenExp = match.tokenDefinition.GetExpansion();
                if (tokenExp.BypassesMovesCost(puzzleStatus))
                {
                    __result = 0;
                    return false;
                }
            }

            int maxCost = 1;
            foreach (var match in _core.matches)
            {
                var tokenExp = match.tokenDefinition.GetExpansion();
                int matchCost = tokenExp.GetMovesCost(match);
                if (matchCost > maxCost) maxCost = matchCost;
            }

            __result = maxCost;
            return false;
        }
    }
}
