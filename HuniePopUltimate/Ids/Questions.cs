using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Questions
{
    public static RelativeId LastName => _lastName;
    private static readonly RelativeId _lastName = new RelativeId(Plugin.ModId, 0);

    public static RelativeId Education => _education;
    private static readonly RelativeId _education = new RelativeId(Plugin.ModId, 1);

    public static RelativeId HomeWorld => _homeWorld;
    private static readonly RelativeId _homeWorld = new RelativeId(Plugin.ModId, 2);

    public static RelativeId Height => _height;
    private static readonly RelativeId _height = new RelativeId(Plugin.ModId, 3);

    public static RelativeId Weight => _weight;
    private static readonly RelativeId _weight = new RelativeId(Plugin.ModId, 4);

    public static RelativeId Occupation => _occupation;
    private static readonly RelativeId _occupation = new RelativeId(Plugin.ModId, 5);

    public static RelativeId CupSize => _cupSize;
    private static readonly RelativeId _cupSize = new RelativeId(Plugin.ModId, 6);

    public static RelativeId Birthday => _birthday;
    private static readonly RelativeId _birthday = new RelativeId(Plugin.ModId, 7);

    public static RelativeId Hobby => _hobby;
    private static readonly RelativeId _hobby = new RelativeId(Plugin.ModId, 8);

    public static RelativeId FavColour => _favColour;
    private static readonly RelativeId _favColour = new RelativeId(Plugin.ModId, 9);

    public static RelativeId FavSeason => _favSeason;
    private static readonly RelativeId _favSeason = new RelativeId(Plugin.ModId, 10);

    public static RelativeId FavHangout => _favHangout;
    private static readonly RelativeId _favHangout = new RelativeId(Plugin.ModId, 11);

    public static RelativeId Age => _age;
    private static readonly RelativeId _age = new RelativeId(Plugin.ModId, 12);
}
