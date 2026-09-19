namespace Hp2BaseMod;

public static class GameStateId
{
    public static RelativeId Sim { get; } = new RelativeId(-1, 0);
    public static RelativeId Puzzle { get; } = new RelativeId(-1, 1);
    public static RelativeId Hub { get; } = new RelativeId(-1, 5);
    public static RelativeId Special { get; } = new RelativeId(-1, 6);
    public static RelativeId Title { get; } = new RelativeId(-1, 7);
}
