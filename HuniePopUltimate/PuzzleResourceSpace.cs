using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hp2BaseMod;

namespace HuniePopUltimate;

public class PuzzleResourceSpace : BasePuzzleResource
{
    private const int BASE_POWER_TOKEN_MULT = 3;

    public override RelativeId Id => _resourceId;
    private readonly RelativeId _resourceId = new RelativeId(Plugin.ModId, 0);
    private readonly RelativeId _affectionId = new RelativeId(Plugin.ModId, 0);

    public PuzzleResourceSpace() : base("Space", "") {}

    public override bool AddResourceValue(int value, ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
        puzzleStatus._affection = Mathf.Clamp(puzzleStatus._affection + value, 0, puzzleStatus._affectionGoal);
        return true;
    }

    public override int GetResourceValue(ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl, bool maxVal = false)
    {
        return maxVal ? puzzleStatus.Core.affectionGoal : puzzleStatus.Core.affection;
    }

    public override bool IsMostFav(PuzzleStatusGirl statusGirl)
    {
        if (statusGirl?.girlDefinition == null || statusGirl.girlDefinition.bossCharacter) return false;

        var girlId = statusGirl.girlDefinition.ModId();

        return girlId == Girls.Celeste;
    }

    public override bool IsLeastFav(PuzzleStatusGirl statusGirl)
    {
        if (statusGirl?.girlDefinition == null || statusGirl.girlDefinition.bossCharacter) return false;

        var girlId = statusGirl.girlDefinition.ModId();

        return girlId != Girls.Celeste;
    }

    public override EnergyDefinition GetEnergyDefinition(PuzzleStatusGirl statusGirl)
    {
        var tokenDef = Game.Data.Tokens.GetAll()
            .FirstOrDefault(t => t.GetExpansion().PuzzleResource == this);
            
        return tokenDef?.energyDefinition;
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
            matchModifier.skipMostFavFactor = true;
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

        rewardAmount += Mathf.RoundToInt(rewardAmount * 4f);
        rewardAmount += Mathf.RoundToInt(rewardAmount * (playerFile.passionMultiplier * (statusGirl.passion * 0.01f)));
        return rewardAmount;
    }

    public override void HandleReplaceDefinition(int absoluteRewardAmount, 
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        IPuzzleReward puzzleReward, 
        List<UiPuzzleSlot> sortedSlots)
    {
        puzzleReward.ReplaceDefinition = match.tokenDefinition;
        puzzleReward.ReplaceUpgraded = true;

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
        var isMostFav = IsMostFav(statusGirl);
        var isLeastFav = IsLeastFav(statusGirl);

        if (matchModifier.skipMostFavFactor) isMostFav = false;
        if (matchModifier.skipLeastFavFactor) isLeastFav = false;

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