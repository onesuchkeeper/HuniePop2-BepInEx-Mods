using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavSundayMorning
{
    public static readonly RelativeId VisitFriends = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.SundayMorning, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {VisitFriends, "Visit Friends"},
            }
        });
    }
}