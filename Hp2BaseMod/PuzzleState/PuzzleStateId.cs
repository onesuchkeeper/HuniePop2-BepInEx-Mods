// Hp2BaseMod 2026, By OneSuchKeeper


namespace Hp2BaseMod;

public static class PuzzleStateId
{
    public static RelativeId Normal { get; } = new RelativeId(-1, 0);
    public static RelativeId Boss { get; } = new RelativeId(-1, 1);
    public static RelativeId Nonstop { get; } = new RelativeId(-1, 2);
    public static RelativeId Tutorial { get; } = new RelativeId(-1, 3);
}
