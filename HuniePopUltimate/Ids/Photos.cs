using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Photos
{
    public static RelativeId KyuOld => _kyuOld;
    private static readonly RelativeId _kyuOld = new RelativeId(Plugin.ModId, 0);

    public static RelativeId Audrey10th => _audrey10th;
    private static readonly RelativeId _audrey10th = new RelativeId(Plugin.ModId, 1);

    internal static int Count = 2;
}
