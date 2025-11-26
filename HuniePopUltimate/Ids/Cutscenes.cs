using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Cutscenes
{
    public static RelativeId PreSex => _preSex;
    private static readonly RelativeId _preSex = new RelativeId(Plugin.ModId, 0);

    public static RelativeId PostSex => _postSex;
    private static readonly RelativeId _postSex = new RelativeId(Plugin.ModId, 1);

    public static RelativeId SuccessAttracted => _successAttracted;
    private static readonly RelativeId _successAttracted = new RelativeId(Plugin.ModId, 2);

    public static RelativeId BonusRoundSuccess => _bonusRoundSuccess;
    private static readonly RelativeId _bonusRoundSuccess = new RelativeId(Plugin.ModId, 3);
}
