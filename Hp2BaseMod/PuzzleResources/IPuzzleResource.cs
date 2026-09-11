using System.Collections.Generic;

namespace Hp2BaseMod
{
    public interface IPuzzleResource
    {
        RelativeId Id { get; }

        int CalculateRewardPortion(int totalRewardAmount, int count, int tokenIndex);

        int CalculateMatchReward(
            ExpandedUiPuzzleGrid uiPuzzleGrid,
            ExpandedAilmentManager ailmentManager,
            ExpandedPuzzleSet puzzleSet,
            PlayerFile playerFile,
            PuzzleMatch match, 
            MatchModifier matchModifier,
            PuzzleStatusGirl statusGirl, 
            List<UiPuzzleSlot> orderedMatchSlots);

        void HandleReplaceDefinition(
            int absoluteRewardAmount, 
            PuzzleMatch match, 
            MatchModifier matchModifier, 
            IPuzzleReward puzzleReward, 
            List<UiPuzzleSlot> sortedSlots);

        bool AddResourceValue(
            int value, 
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl);

        int GetResourceValue(
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl, 
            bool maxVal = false);

        bool IsMostFav(PuzzleStatusGirl statusGirl);

        bool IsLeastFav(PuzzleStatusGirl statusGirl);

        EnergyDefinition GetEnergyDefinition(PuzzleStatusGirl statusGirl);

        (string splashText, string burstText) GetLabelText(
            int resourceValue,
            bool bonusRound = false);
    }
}
