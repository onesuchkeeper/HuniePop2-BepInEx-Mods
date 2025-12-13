using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Weight
{
    public static RelativeId _112 = new RelativeId(Plugin.ModId, 0);
    public static RelativeId _109 = new RelativeId(Plugin.ModId, 1);
    public static RelativeId _118 = new RelativeId(Plugin.ModId, 2);
    public static RelativeId _102 = new RelativeId(Plugin.ModId, 3);
    public static RelativeId _104 = new RelativeId(Plugin.ModId, 4);
    public static RelativeId _132 = new RelativeId(Plugin.ModId, 5);
    public static RelativeId _100 = new RelativeId(Plugin.ModId, 6);
    public static RelativeId _130 = new RelativeId(Plugin.ModId, 7);
    public static RelativeId _110 = new RelativeId(Plugin.ModId, 8);
    public static RelativeId _128 = new RelativeId(Plugin.ModId, 9);
    public static RelativeId _122 = new RelativeId(Plugin.ModId, 10);
    public static RelativeId _126 = new RelativeId(Plugin.ModId, 11);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Weight, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Weight",
            QuestionText = "How much [[highlight]do you weigh]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {_112, "112 lbs"},
                {_109, "109 lbs"},
                {_118, "118 lbs"},
                {_102, "102 lbs"},
                {_104, "104 lbs"},
                {_132, "132 lbs"},
                {_100, "100 lbs"},
                {_130, "130 lbs"},
                {_110, "110 lbs"},
                {_128, "128 lbs"},
                {_122, "122 lbs"},
                {_126, "126 lbs"},
            }
        });
    }
}