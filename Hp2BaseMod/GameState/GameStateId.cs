namespace Hp2BaseMod;

public static class GameStateId
{
    public static RelativeId Sim { get; } = new RelativeId(-1, 0);
    public static RelativeId Date { get; } = new RelativeId(-1, 1);
    public static RelativeId Hub { get; } = new RelativeId(-1, 2);
    public static RelativeId Special { get; } = new RelativeId(-1, 3);
    public static RelativeId Title { get; } = new RelativeId(-1, 4);
}