using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// Ids of the item gift handlers this mod adds. All ids are local to this mod.
/// </summary>
public static class ItemGiftHandlers
{
    /// <summary>Handler for giving the sensitivity smoothie.</summary>
    public static RelativeId SensitivitySmoothie => _sensitivitySmoothie;
    private static readonly RelativeId _sensitivitySmoothie = new RelativeId(Plugin.ModId, 0);
}
