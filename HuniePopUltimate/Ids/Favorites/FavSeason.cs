using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavSeason
{
    public static RelativeId Summer = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Autumn = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Spring = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Winter = new RelativeId(Plugin.ModId, 3);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.FavSeason, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Season",
            QuestionText = "What is your favorite [[highlight]season]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Summer, "Summer"},
                {Autumn, "Autumn"},
                {Spring, "Spring"},
                {Winter, "Winter"},
            }
        });
    }
}