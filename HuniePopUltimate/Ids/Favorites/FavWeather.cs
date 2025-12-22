using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavWeather
{
    public static readonly RelativeId Calm = new RelativeId(Plugin.ModId, 0);
    public static readonly RelativeId CosmicStorm = new RelativeId(Plugin.ModId, 1);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.Weather, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Calm, "Calm"},
                {CosmicStorm, "Cosmic Storm"},
            }
        });
    }
}