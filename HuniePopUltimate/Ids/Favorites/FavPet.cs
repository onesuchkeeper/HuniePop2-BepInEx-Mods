using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavPet
{
    public static readonly RelativeId Hamster = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.Pet, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Hamster, "Hamster"},
            }
        });
    }
}