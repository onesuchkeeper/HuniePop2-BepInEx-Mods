using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavOwnBodyPart
{
    public static readonly RelativeId Hair = new RelativeId(Plugin.ModId, 0);
    public static readonly RelativeId Horns = new RelativeId(Plugin.ModId, 1);
    public static readonly RelativeId Wings = new RelativeId(Plugin.ModId, 2);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.BodyPart, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Hair, "Hair"},
                {Horns, "Horns"},
                {Wings, "Wings"},
            }
        });
    }
}