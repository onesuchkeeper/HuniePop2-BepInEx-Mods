// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

public interface IPuzzleStateFactory
{
    public IFullPuzzleState Create(IGameStateLocation gameState);
}