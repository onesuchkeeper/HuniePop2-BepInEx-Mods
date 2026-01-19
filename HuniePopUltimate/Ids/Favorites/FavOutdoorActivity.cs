using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavOutdoorActivity
{
    public static readonly RelativeId Biking = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.OutdoorActivity, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Biking, "Biking"},
            }
        });
    }
}