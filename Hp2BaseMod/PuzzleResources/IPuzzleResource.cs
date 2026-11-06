using System.Collections.Generic;

namespace Hp2BaseMod
{
    public interface IPuzzleResource
    {
        /// <summary>
        /// Unique identifier of the puzzle resource
        /// </summary>
        RelativeId Id { get; }

        /// <summary>
        /// Calculates what ammount of the reward goes to the token at the given index
        /// </summary>
        /// <param name="totalRewardAmount"></param>
        /// <param name="count"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        int CalculateRewardPortion(int totalRewardAmount, int count, int tokenIndex);

        /// <summary>
        /// Calculates the reward from a match of this resource's type
        /// </summary>
        /// <param name="uiPuzzleGrid"></param>
        /// <param name="ailmentManager"></param>
        /// <param name="puzzleSet"></param>
        /// <param name="playerFile"></param>
        /// <param name="match"></param>
        /// <param name="matchModifier"></param>
        /// <param name="statusGirl"></param>
        /// <param name="orderedMatchSlots"></param>
        /// <returns></returns>
        int CalculateMatchReward(
            ExpandedUiPuzzleGrid uiPuzzleGrid,
            ExpandedAilmentManager ailmentManager,
            ExpandedPuzzleSet puzzleSet,
            PlayerFile playerFile,
            PuzzleMatch match, 
            MatchModifier matchModifier,
            PuzzleStatusGirl statusGirl, 
            List<UiPuzzleSlot> orderedMatchSlots);

        /// <summary>
        /// Handles replacements from matches of the resource's type
        /// </summary>
        /// <param name="absoluteRewardAmount"></param>
        /// <param name="match"></param>
        /// <param name="matchModifier"></param>
        /// <param name="puzzleReward"></param>
        /// <param name="sortedSlots"></param>
        void HandleReplaceDefinition(
            int absoluteRewardAmount, 
            PuzzleMatch match, 
            MatchModifier matchModifier, 
            IPuzzleReward puzzleReward, 
            List<UiPuzzleSlot> sortedSlots);

        /// <summary>
        /// Adds resources to the puzzle
        /// </summary>
        /// <param name="value"></param>
        /// <param name="puzzleStatus"></param>
        /// <param name="puzzleStatusGirl"></param>
        /// <returns></returns>
        bool AddResourceValue(
            int value, 
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="puzzleStatus"></param>
        /// <param name="puzzleStatusGirl"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        int GetResourceValue(
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl, 
            bool maxVal = false);

        //TODO checking most or least favorite only applies to affection tokens, but there are abilities that need to be able to check
        //maybe give those abilities the explicite resource id instead?

        /// <summary>
        /// Checks if this resource is of the girl's favorite
        /// </summary>
        /// <param name="statusGirl"></param>
        /// <returns></returns>
        bool IsMostFav(PuzzleStatusGirl statusGirl);

        /// <summary>
        /// Check if this resource is of the girl's least favorite
        /// </summary>
        /// <param name="statusGirl"></param>
        /// <returns></returns>
        bool IsLeastFav(PuzzleStatusGirl statusGirl);

        /// <summary>
        /// Gets the energy definition to be used when displaying this resource
        /// </summary>
        /// <param name="statusGirl"></param>
        /// <returns></returns>
        EnergyDefinition GetEnergyDefinition(PuzzleStatusGirl statusGirl);

        /// <summary>
        /// Gets the lables for text describing this resource
        /// </summary>
        /// <param name="resourceValue"></param>
        /// <param name="bonusRound"></param>
        /// <returns></returns>
        (string splashText, string burstText) GetLabelText(
            int resourceValue,
            bool bonusRound = false);
    }
}
