using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavHoliday
{
    public static readonly RelativeId None = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.Holiday, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {None, "None"},
            }
        });
    }
}