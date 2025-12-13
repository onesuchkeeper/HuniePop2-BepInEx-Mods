using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Age
{
    public static RelativeId _28 = new RelativeId(Plugin.ModId, 0);
    public static RelativeId _21 = new RelativeId(Plugin.ModId, 1);
    public static RelativeId _23 = new RelativeId(Plugin.ModId, 2);
    public static RelativeId _32 = new RelativeId(Plugin.ModId, 3);
    public static RelativeId _36 = new RelativeId(Plugin.ModId, 4);
    public static RelativeId _384 = new RelativeId(Plugin.ModId, 6);
    public static RelativeId _24 = new RelativeId(Plugin.ModId, 7);
    public static RelativeId _1CatYears = new RelativeId(Plugin.ModId, 8);
    public static RelativeId _18 = new RelativeId(Plugin.ModId, 9);
    public static RelativeId _20 = new RelativeId(Plugin.ModId, 10);
    public static RelativeId _10000 = new RelativeId(Plugin.ModId, 5);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Age, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Age",
            QuestionText = "How [[highlight]old are you]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {_28, "28"},
                {_21, "21"},
                {_23, "23"},
                {_32, "32"},
                {_36, "36"},
                {_384, "384"},
                {_24, "24"},
                {_1CatYears, "1 (Cat Years)"},
                {_18, "18"},
                {_20, "20"},
                {_10000, ">10,000"}
            }
        });
    }
}
