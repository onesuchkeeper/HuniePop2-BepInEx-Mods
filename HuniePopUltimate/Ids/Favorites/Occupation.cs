using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Occupation
{
    public static RelativeId Student = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Professor = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Hairdresser = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Barista = new RelativeId(Plugin.ModId, 3);
    public static RelativeId YogaTeacher = new RelativeId(Plugin.ModId, 4);
    public static RelativeId Kitty = new RelativeId(Plugin.ModId, 5);
    public static RelativeId Hunter = new RelativeId(Plugin.ModId, 6);
    public static RelativeId LoveFairy = new RelativeId(Plugin.ModId, 7);
    public static RelativeId Goddess = new RelativeId(Plugin.ModId, 8);
    public static RelativeId Stewardess = new RelativeId(Plugin.ModId, 9);
    public static RelativeId PornStar = new RelativeId(Plugin.ModId, 10);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Occupation, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Occupation",
            QuestionText = "What do you [[highlight]do for work]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Student, "Student"},
                {Professor, "Professor"},
                {Hairdresser, "Hairdresser"},
                {Barista, "Barista"},
                {YogaTeacher, "Yoga Teacher"},
                {Kitty, "Kitty"},
                {Hunter, "Bounty Hunter"},
                {LoveFairy, "Love Fairy"},
                {Goddess, "Goddess"},
                {Stewardess, "Stewardess"},
                {PornStar, "Porn Star"},
            }
        });
    }
}