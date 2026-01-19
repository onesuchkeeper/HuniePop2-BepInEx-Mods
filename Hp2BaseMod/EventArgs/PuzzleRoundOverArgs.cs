namespace Hp2BaseMod;

public class PuzzleRoundOverArgs
{
    public enum CutsceneType
    {
        None,
        AttractToLovers,
        CompatToAttract
    }

    public CutsceneType LevelUpType;
    public bool IsSexDate;
    public bool IsGameOver;
    public bool IsSuccess;

    public override string ToString()
        => $"IsSexDate: {IsSexDate} IsGameOver: {IsGameOver}, IsSuccess: {IsSuccess}, LevelUpType: {LevelUpType}";
}
