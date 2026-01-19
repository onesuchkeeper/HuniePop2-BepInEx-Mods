using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavExercise
{
    public static readonly RelativeId Hunting = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Hp2BaseMod.Favorites.Exercise, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Hunting, "Hunting"},
            }
        });
    }
}