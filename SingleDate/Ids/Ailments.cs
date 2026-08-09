using Hp2BaseMod;

namespace SingleDate;

public static class Ailments
{
    public static RelativeId SingleDateAilment => _singleDateAilment;
    private static readonly RelativeId _singleDateAilment = new RelativeId(Plugin.ModId, 0);
}
