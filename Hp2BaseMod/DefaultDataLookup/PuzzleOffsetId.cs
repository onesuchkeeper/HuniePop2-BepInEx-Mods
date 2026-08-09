using System.Collections.Generic;

namespace Hp2BaseMod;

public static class PuzzleOffsetId
{
    private static readonly Dictionary<RelativeId, string> _offsetNames = new()
    {
        {PowerTokenMult, "power_token_multiplier"},
        {DateGiftUseCost, "date_gift_use_cost"},
        {ExhaustedRecoveryRate, "exhausted_recovery_rate"},
        {PowerTokenChance, "power_token_chance"},
        {FlipMostLeastFavs, "flip_most_least_favs"},
    };

    public static RelativeId PowerTokenMult => new RelativeId(-1, 0);
    public static RelativeId DateGiftUseCost => new RelativeId(-1, 1);
    public static RelativeId ExhaustedRecoveryRate => new RelativeId(-1, 2);
    public static RelativeId PowerTokenChance => new RelativeId(-1, 3);
    public static RelativeId FlipMostLeastFavs => new RelativeId(-1, 4);

    public static int GetPuzzleOffset(this PuzzleManager puzzleManager, RelativeId puzzleOffsetId)
    {
        if (_offsetNames.TryGetValue(puzzleOffsetId, out var puzzleOffsetName)) return puzzleManager.GetPuzzleOffset(puzzleOffsetName);
        return 0;
    }

    public static bool IsPuzzleOffset(this PuzzleManager puzzleManager, RelativeId puzzleOffsetId)
    {
        if (_offsetNames.TryGetValue(puzzleOffsetId, out var puzzleOffsetName)) return puzzleManager.IsPuzzleOffset(puzzleOffsetName);
        return false;
    }

    public static bool AdjustPuzzleOffset(this PuzzleManager puzzleManager, RelativeId puzzleOffsetId, int offsetAmount)
    {
        if (_offsetNames.TryGetValue(puzzleOffsetId, out var puzzleOffsetName))
        {
            puzzleManager.AdjustPuzzleOffset(puzzleOffsetName, offsetAmount); 
            return true;
        }

        return false;
    }
}
