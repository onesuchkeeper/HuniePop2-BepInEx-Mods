using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// Ids of the girls this mod adds. All ids are local to this mod.
/// </summary>
public static class Girls
{
    /// <summary>The empty girl that is girl one of every single pair.</summary>
    public static RelativeId Nobody => _nobody;
    private static readonly RelativeId _nobody = new RelativeId(Plugin.ModId, 0);
}
