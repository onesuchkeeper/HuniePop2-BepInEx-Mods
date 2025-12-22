using Hp2BaseMod;
namespace SingleDate;

public static class CutsceneIds
{
    public static RelativeId Meeting => _meeting;
    private static RelativeId _meeting = new RelativeId(State.ModId, 0);

    public static RelativeId Attract => _attract;
    private static RelativeId _attract = new RelativeId(State.ModId, 1);

    public static RelativeId PreSex => _preSex;
    private static RelativeId _preSex = new RelativeId(State.ModId, 2);

    public static RelativeId PostSex => _postSex;
    private static RelativeId _postSex = new RelativeId(State.ModId, 3);

    public static RelativeId Success => _success;
    private static RelativeId _success = new RelativeId(State.ModId, 4);

    public static RelativeId BonusSuccess => _bonusSuccess;
    private static RelativeId _bonusSuccess = new RelativeId(State.ModId, 5);
}