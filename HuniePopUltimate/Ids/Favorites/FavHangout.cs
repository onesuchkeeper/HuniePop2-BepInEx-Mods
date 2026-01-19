using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavHangout
{
    public static RelativeId Campus = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Casino = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Gym = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Nightclub = new RelativeId(Plugin.ModId, 3);
    public static RelativeId Home = new RelativeId(Plugin.ModId, 4);
    public static RelativeId Park = new RelativeId(Plugin.ModId, 5);
    public static RelativeId HikingTrail = new RelativeId(Plugin.ModId, 6);
    public static RelativeId Beach = new RelativeId(Plugin.ModId, 7);
    public static RelativeId Bedroom = new RelativeId(Plugin.ModId, 8);
    public static RelativeId HotSpring = new RelativeId(Plugin.ModId, 9);
    public static RelativeId Cafe = new RelativeId(Plugin.ModId, 10);
    public static RelativeId Bar = new RelativeId(Plugin.ModId, 11);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.FavHangout, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Hangout",
            QuestionText = "What is your favorite [[highlight]place to hang out]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Campus, "Campus"},
                {Casino, "Casino"},
                {Gym, "Gym"},
                {Nightclub, "Nightclub"},
                {Home, "Home"},
                {Park, "Park"},
                {HikingTrail, "Hiking Trail"},
                {Beach, "Beach"},
                {Bedroom, "Bedroom"},
                {HotSpring, "Hot Spring"},
                {Cafe, "Café"},
                {Bar, "Bar"},
            }
        });
    }
}