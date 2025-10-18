using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;
public static class FavShop
{
    public static readonly RelativeId HobbyShop = new RelativeId(Plugin.ModId, 0);
    public static readonly RelativeId Bakery = new RelativeId(Plugin.ModId, 1);
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.Shop, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {HobbyShop, "Hobby Shop"},
                {Bakery, "Bakery"},
            }
        });
    }
}