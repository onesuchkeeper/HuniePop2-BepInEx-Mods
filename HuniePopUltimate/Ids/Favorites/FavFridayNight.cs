using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavFridayNight
{
    public static readonly RelativeId Reading = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.FridayNight, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Reading, "Reading"},
            }
        });
    }
}