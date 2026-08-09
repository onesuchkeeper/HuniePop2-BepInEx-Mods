using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hp2BaseMod;

public class PuzzleResourceAffection : BasePuzzleResource
{
    private const int BASE_POWER_TOKEN_MULT = 3;

    public override RelativeId Id => _resourceId;
    private readonly RelativeId _resourceId;

    private readonly RelativeId _affectionId;
    public PuzzleResourceAffection(string name, string sign, RelativeId resourceId, RelativeId affectionId)
        : base(name, sign)
    {
        _resourceId = resourceId;
        _affectionId = affectionId;
    }

    public override bool AddResourceValue(int value, 
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;

		puzzleStatus._affection = Mathf.Clamp(puzzleStatus._affection + value, 0, puzzleStatus._affectionGoal);
		return true;
    }

    public override int CalculateMatchReward(
        ExpandedUiPuzzleGrid uiPuzzleGrid,
        ExpandedAilmentManager ailmentManager,
        ExpandedPuzzleSet puzzleSet,
        PlayerFile playerFile,
        PuzzleMatch match, 
        MatchModifier matchModifier,
        PuzzleStatusGirl statusGirl, 
        List<UiPuzzleSlot> orderedMatchSlots)
    {
        if (statusGirl.girlDefinition.bossCharacter)
        {
            matchModifier.skipLeastFavFactor = true;
            matchModifier.skipLeastFavFactor = true;
        }

        var args = NotifyPreMatch(uiPuzzleGrid,
            ailmentManager,
            puzzleSet,
            match, 
            matchModifier,
            statusGirl, 
            orderedMatchSlots);

        var rewardAmount = orderedMatchSlots.Count;

        if (!match.flatMatch)
        {
            rewardAmount *= Mathf.Max(rewardAmount - 2, 1);
        }

        var effectiveMostFav = args.IsMostFav
            && !args.IsLeastFav
            && !matchModifier.skipMostFavFactor;

        var effectiveLeastFav = args.IsLeastFav
            && !args.IsMostFav
            && !matchModifier.skipLeastFavFactor;

        if (effectiveMostFav)
        {
            rewardAmount *= 3;
        }
        else if (!effectiveLeastFav)
        {
            rewardAmount *= 2;
        }

        var multPerPowerToken = Mathf.Max(
            BASE_POWER_TOKEN_MULT + Game.Session.Puzzle.GetPuzzleOffset(PuzzleOffsetId.PowerTokenMult), 
            0);
        var powerMult = multPerPowerToken * orderedMatchSlots.Count(x => x.token.upgraded);

        if (powerMult > 0) rewardAmount *= powerMult;

        rewardAmount += Mathf.RoundToInt(rewardAmount * (4f * (playerFile.GetAffectionLevelExp(_affectionId) / 24f)));
        rewardAmount += Mathf.RoundToInt(rewardAmount * (playerFile.passionMultiplier * (statusGirl.passion * 0.01f)));
        return rewardAmount;
    }

    public override void HandleReplaceDefinition(int absoluteRewardAmount, 
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        IPuzzleReward puzzleReward, 
        List<UiPuzzleSlot> sortedSlots)
    {
        if (sortedSlots.Count > 2 && absoluteRewardAmount > 0)
        {
            var powerTokenChance = Game.Session.Puzzle.GetPuzzleOffset(PuzzleOffsetId.PowerTokenChance);
            var powerTokenOdds = Mathf.Clamp(sortedSlots.Count switch
            {
                0 or
                1 or
                2 or
                3 => 0f + 0.1f * Game.Persistence.playerFile.styleFactor + powerTokenChance * 0.025f,

                4 => 0.2f + 0.6f * Game.Persistence.playerFile.styleFactor + powerTokenChance * 0.15f,
                5 => 0.8f + 0.2f * Game.Persistence.playerFile.styleFactor + powerTokenChance * 0.05f,
                _ => 1f,
            }, 0f, 1f);

            //powerTokenOdds = NotifyPreReplaceDefinition(match, matchModifier, sortedSlots, powerTokenOdds, out var canceled);          

            if (Random.Range(0f, 1f) <= powerTokenOdds)
            {
                puzzleReward.ReplaceDefinition = match.tokenDefinition;
                puzzleReward.ReplaceUpgraded = true;
            }
        }

        base.HandleReplaceDefinition(absoluteRewardAmount, match, matchModifier, puzzleReward, sortedSlots);
    }

    private new AilmentTriggerArgs.PreAffectionMatchReward NotifyPreMatch(ExpandedUiPuzzleGrid uiPuzzleGrid, 
        ExpandedAilmentManager ailmentManager, 
        ExpandedPuzzleSet puzzleSet,
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        PuzzleStatusGirl statusGirl, 
        List<UiPuzzleSlot> orderedMatchSlots)
    {
        var isMostFav = false;
        var isLeastFav = false;

        var girlExp = statusGirl.girlDefinition.GetExpansion();

        if (!statusGirl.girlDefinition.bossCharacter)
        {
            isMostFav = !matchModifier.skipMostFavFactor
                && _affectionId == girlExp.GetMostFavAffectionType().Id;

            isLeastFav = !matchModifier.skipLeastFavFactor
                && _affectionId == girlExp.GetLeastFavAffectionType().Id;
        }
        
        var args = new AilmentTriggerArgs.PreAffectionMatchReward(uiPuzzleGrid, 
            statusGirl, 
            match, 
            matchModifier,
            _affectionId,
            isMostFav,
            isLeastFav,
            orderedMatchSlots);

        ailmentManager.OnPreAffectionMatchReward(args);

        foreach (var excludedSlot in args.ExcludedSlots)
        {
            orderedMatchSlots.Remove(excludedSlot);
            puzzleSet.Core.allSlots.Remove(excludedSlot);
        }

        return args;
    }
}
