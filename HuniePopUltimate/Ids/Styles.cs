using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Styles
{
    public static RelativeId Topless => _topless;
    private static readonly RelativeId _topless = new RelativeId(Plugin.ModId, 0);

    public static RelativeId KyuDisguise => _kyuDisguise;
    private static readonly RelativeId _kyuDisguise = new RelativeId(Plugin.ModId, 1);
}
