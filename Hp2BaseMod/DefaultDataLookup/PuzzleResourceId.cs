// Hp2BaseMod 2025, By OneSuchKeeper

using System;

namespace Hp2BaseMod;
public static class PuzzleResourceId
{
    public static RelativeId AffectionFlirtation => new RelativeId(-1, 0);
    public static RelativeId AffectionTalent => new RelativeId(-1, 1);
    public static RelativeId AffectionRomance => new RelativeId(-1, 2);
    public static RelativeId AffectionSexuality => new RelativeId(-1, 3);
    public static RelativeId Moves => new RelativeId(-1, 4);
    public static RelativeId Stamina => new RelativeId(-1, 5);
    public static RelativeId Passion => new RelativeId(-1, 6);
    public static RelativeId Sentiment => new RelativeId(-1, 7);
    public static RelativeId Broken => new RelativeId(-1, 8);

    internal static RelativeId From(PuzzleResourceType puzzleResourceType)
    {
        switch (puzzleResourceType)
        {
            case PuzzleResourceType.MOVES:
                return Moves;
            case PuzzleResourceType.STAMINA:
                return Stamina;
            case PuzzleResourceType.PASSION:
                return Passion;
            case PuzzleResourceType.SENTIMENT:
                return Sentiment;
            case PuzzleResourceType.BROKEN:
                return Broken;
        }

        throw new Exception($"Unhandled {typeof(PuzzleResourceType).Name} {puzzleResourceType}");
    }
}
