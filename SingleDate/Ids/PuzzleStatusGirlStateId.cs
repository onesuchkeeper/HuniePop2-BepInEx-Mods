using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// Ids of the puzzle girl states this mod adds. All ids are local to this mod.
/// </summary>
public static class PuzzleStatusGirlStateId
{
    /// <summary>The upset state used on single dates, see <see cref="SingleDateUpsetGirlState"/>.</summary>
    public static RelativeId SingleDateUpset => _singleDateUpset;
    private static readonly RelativeId _singleDateUpset = new RelativeId(Plugin.ModId, 0);
}
