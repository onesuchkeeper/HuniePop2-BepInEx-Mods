using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class LastName
{
    public static RelativeId Maye = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Yumi = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Delrio = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Belrose = new RelativeId(Plugin.ModId, 3);
    public static RelativeId AnnMarie = new RelativeId(Plugin.ModId, 4);
    public static RelativeId Lapran = new RelativeId(Plugin.ModId, 5);
    public static RelativeId NotApplicable = new RelativeId(Plugin.ModId, 6);
    public static RelativeId Luvendass = new RelativeId(Plugin.ModId, 7);
    public static RelativeId Sugardust = new RelativeId(Plugin.ModId, 8);
    public static RelativeId Venus = new RelativeId(Plugin.ModId, 9);
    public static RelativeId Rembrite = new RelativeId(Plugin.ModId, 10);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.LastName, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Last Name",
            QuestionText = "What is your [[highlight]last name]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
             {
                {Maye, "Maye"},
                {Yumi, "Yumi"},
                {Delrio, "Delrio"},
                {Belrose, "Belrose"},
                {AnnMarie, "Ann-Marie"},
                {Lapran, "Lapran"},
                {NotApplicable, "N/A"},
                {Luvendass, "Luvendass"},
                {Sugardust, "Sugardust"},
                {Venus, "Venus"},
                {Rembrite, "Rembrite"},
             }
        });
    }
}