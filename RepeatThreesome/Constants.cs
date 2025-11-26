// RepeatThreesome 2021, By OneSuchKeeper

using Hp2BaseMod;
namespace RepeatThreesome;

public static class Constants
{
    public static RelativeId LocalCodeId;
    public static RelativeId NudeCodeId;
    public static RelativeId NudeOutfitId;

    internal static void Init(int modId)
    {
        LocalCodeId = new RelativeId(modId, 0);
        NudeCodeId = new RelativeId(modId, 1);

        NudeOutfitId = new RelativeId(modId, 0);
    }
}
