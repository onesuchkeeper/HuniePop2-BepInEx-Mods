using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavDrink
{
    public static RelativeId Soda = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Juice = new RelativeId(Plugin.ModId, 1);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.Drink, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Soda, "Soda"},
                {Juice, "Juice"},
            }
        });
    }
}