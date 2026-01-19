using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavIceCream
{
    public static readonly RelativeId Carmel = new RelativeId(Plugin.ModId, 0);
    public static readonly RelativeId None = new RelativeId(Plugin.ModId, 1);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.IceCream, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Carmel, "Carmel"},
                {None, "none"},
            }
        });
    }
}