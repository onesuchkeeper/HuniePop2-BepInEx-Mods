using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;
public static class FavSexPos
{
    public static readonly RelativeId Grinding = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.SexPos, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Grinding, "Grinding"},
            }
        });
    }
}