using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// Ids of the ailments this mod adds. All ids are local to this mod.
/// </summary>
public static class Ailments
{
    /// <summary>The scripted ailment enabled on every single date, see <see cref="SingleDate.SingleDateAilment"/>.</summary>
    public static RelativeId SingleDateAilment => _singleDateAilment;
    private static readonly RelativeId _singleDateAilment = new RelativeId(Plugin.ModId, 0);
}
