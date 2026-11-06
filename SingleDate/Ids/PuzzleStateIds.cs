using Hp2BaseMod;

namespace SingleDate;

public static class PuzzleStateIds
{
    public static RelativeId SingleDatePuzzle => _singleDatePuzzle;
    private static readonly RelativeId _singleDatePuzzle = new RelativeId(Plugin.ModId, 0);
}
