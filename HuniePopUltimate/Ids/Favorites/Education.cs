using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Education
{
    public static RelativeId InCollege = new RelativeId(Plugin.ModId, 0);
    public static RelativeId College_6 = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Dropout = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Highschool = new RelativeId(Plugin.ModId, 3);
    public static RelativeId College_2 = new RelativeId(Plugin.ModId, 4);
    public static RelativeId College_4 = new RelativeId(Plugin.ModId, 5);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Education, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Education",
            QuestionText = "What kind of [[highlight]education do you have]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {InCollege, "In College"},
                {College_6, "College (6 years)"},
                {Dropout, "Dropout"},
                {Highschool, "Highschool"},
                {College_2, "College (2 years)"},
                {College_4, "College (4 years)"},
            }
        });
    }
}