using Hp2BaseMod;

namespace SingleDate;

public static class GameStateIds
{
    /// <summary>The game state used while the player is with a single pair at a sim location, see <see cref="SingleDate.SingleDateSim"/>.</summary>
    public static RelativeId SingleDateSim => _singleDateSim;
    private static readonly RelativeId _singleDateSim = new RelativeId(Plugin.ModId, 0);
}