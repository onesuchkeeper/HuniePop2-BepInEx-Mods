using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavOnlineActivity
{
    public static readonly RelativeId Newgrounds = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.OnlineActivity, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Newgrounds, "NewGrounds"},
            }
        });
    }
}