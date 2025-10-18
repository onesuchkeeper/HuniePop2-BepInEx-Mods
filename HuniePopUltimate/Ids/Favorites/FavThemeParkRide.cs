using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;
public static class FavThemeParkRide
{
    public static readonly RelativeId BumperCars = new RelativeId(Plugin.ModId, 0);
    public static readonly RelativeId None = new RelativeId(Plugin.ModId, 1);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.ThemeParkRide, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {BumperCars, "Bumper Cars"},
                {None, "None"},
            }
        });
    }
}