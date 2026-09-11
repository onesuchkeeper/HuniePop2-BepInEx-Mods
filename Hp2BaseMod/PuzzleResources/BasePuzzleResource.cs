using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public abstract class BasePuzzleResource : IPuzzleResource
{
    protected readonly string _name;
    protected readonly string _sign;

    public BasePuzzleResource(string name, string sign)
    {
        _name = name;
        _sign = sign;
    }

    public abstract RelativeId Id { get; }

    public abstract bool AddResourceValue(
        int value, 
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl);

    public abstract int CalculateMatchReward(
        ExpandedUiPuzzleGrid uiPuzzleGrid, 
        ExpandedAilmentManager ailmentManager, 
        ExpandedPuzzleSet puzzleSet,
        PlayerFile playerFile, 
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        PuzzleStatusGirl statusGirl, 
        List<UiPuzzleSlot> orderedMatchSlots);

    public virtual int GetResourceValue(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        bool maxVal = false) => 0;

    public virtual bool IsMostFav(PuzzleStatusGirl statusGirl) => false;

    public virtual bool IsLeastFav(PuzzleStatusGirl statusGirl) => false;

    public virtual EnergyDefinition GetEnergyDefinition(PuzzleStatusGirl statusGirl) => null;

    public virtual int CalculateRewardPortion(int totalRewardAmount, int count, int tokenIndex)
    {
        return Mathf.CeilToInt(totalRewardAmount / (float)(count - tokenIndex));
    }

    public virtual void HandleReplaceDefinition(
        int absoluteRewardAmount, 
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        IPuzzleReward puzzleReward, 
        List<UiPuzzleSlot> sortedSlots)
    {
        // Only apply non-priority matchModifier replacements if no replacement (e.g. power token) was set
        if (puzzleReward.ReplaceDefinition == null 
            && matchModifier.replaceDefinition != null 
            && !matchModifier.replacePriority)
        {
            puzzleReward.ReplaceDefinition = matchModifier.replaceDefinition;
        }
    }

    public AilmentTriggerArgs.PreMatchReward NotifyPreMatch(
        ExpandedUiPuzzleGrid uiPuzzleGrid, 
        ExpandedAilmentManager ailmentManager, 
        ExpandedPuzzleSet puzzleSet,
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        PuzzleStatusGirl statusGirl, 
        List<UiPuzzleSlot> orderedMatchSlots)
    {
        var args = new AilmentTriggerArgs.PreMatchReward(
            uiPuzzleGrid,
            statusGirl, 
            match, 
            matchModifier, 
            orderedMatchSlots);

        ailmentManager.OnPreMatchReward(args);

        foreach (var excludedSlot in args.ExcludedSlots)
        {
            orderedMatchSlots.Remove(excludedSlot);
            puzzleSet.Core.allSlots.Remove(excludedSlot);
        }

        return args;
    }

    public virtual (string splashText, string burstText) GetLabelText(
        int resourceValue,
        bool bonusRound = false)
    {
        if (resourceValue == 0) return (null, null);

        string text = (resourceValue >= 0 ? "+" : "-") + Mathf.Abs(resourceValue);

        if (!bonusRound && !StringUtils.IsEmpty(_sign))
        {
            text += _sign;
        }

        text += " ";
        text += Game.Session.Puzzle.puzzleStatus.bonusRound
            ? "Pleasure"
            : _name;

        return (text, text);
    }
}
