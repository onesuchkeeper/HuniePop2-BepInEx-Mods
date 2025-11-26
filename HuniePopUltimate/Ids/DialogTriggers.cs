using Hp2BaseMod;

namespace HuniePopUltimate;

public static class DialogTriggers
{
    public static RelativeId PreBedroom => _preBedroom;
    public static readonly RelativeId _preBedroom = new RelativeId(Plugin.ModId, 0);

    public static RelativeId PreSex => _preSex;
    public static readonly RelativeId _preSex = new RelativeId(Plugin.ModId, 1);

    public static RelativeId PostSex => _postSex;
    public static readonly RelativeId _postSex = new RelativeId(Plugin.ModId, 2);
}