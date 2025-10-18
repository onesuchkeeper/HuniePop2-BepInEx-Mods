using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;
public static class FavMovieGenre
{
    public static readonly RelativeId Drama = new RelativeId(Plugin.ModId, 0);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Hp2BaseMod.Favorites.MovieGenre, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionAnswers = new() {
                {Drama, "Drama"},
            }
        });
    }
}