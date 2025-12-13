using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Height
{
    public static RelativeId _5_4 = new RelativeId(Plugin.ModId, 0);
    public static RelativeId _5_6 = new RelativeId(Plugin.ModId, 1);
    public static RelativeId _5_2 = new RelativeId(Plugin.ModId, 2);
    public static RelativeId _5_0 = new RelativeId(Plugin.ModId, 3);
    public static RelativeId _5_11 = new RelativeId(Plugin.ModId, 4);
    public static RelativeId _5_9 = new RelativeId(Plugin.ModId, 5);
    public static RelativeId _5_8 = new RelativeId(Plugin.ModId, 6);
    public static RelativeId _5_7 = new RelativeId(Plugin.ModId, 7);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Height, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Height",
            QuestionText = "How [[highlight]tall are you]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {_5_4, "5\'4\""},
                {_5_6, "5\'6\""},
                {_5_2, "5\'2\""},
                {_5_0, "5\'0\""},
                {_5_11, "5\'11\""},
                {_5_9, "5\'9\""},
                {_5_8, "5\'8\""},
                {_5_7, "5\'7\""},
            }
        });
    }
}