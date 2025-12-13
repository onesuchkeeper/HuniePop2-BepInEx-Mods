using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Birthday
{
    public static RelativeId Dec_22 = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Nov_9 = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Mar_16 = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Apr_6 = new RelativeId(Plugin.ModId, 3);
    public static RelativeId May_13 = new RelativeId(Plugin.ModId, 4);
    public static RelativeId June_25 = new RelativeId(Plugin.ModId, 5);
    public static RelativeId Oct_2 = new RelativeId(Plugin.ModId, 6);
    public static RelativeId July_20 = new RelativeId(Plugin.ModId, 7);
    public static RelativeId Aug_3 = new RelativeId(Plugin.ModId, 8);
    public static RelativeId Sep_1 = new RelativeId(Plugin.ModId, 9);
    public static RelativeId Feb_23 = new RelativeId(Plugin.ModId, 10);
    public static RelativeId Jan_27 = new RelativeId(Plugin.ModId, 11);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.Birthday, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Birthday",
            QuestionText = "When is [[highlight]your birthday]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Dec_22, "December 22nd"},
                {Nov_9, "November 9th"},
                {Mar_16, "March 16th"},
                {Apr_6, "April 6th"},
                {May_13, "May 13th"},
                {June_25, "June 25th"},
                {Oct_2, "October 2nd"},
                {July_20, "July 20th"},
                {Aug_3, "August 3rd"},
                {Sep_1, "September 1st"},
                {Feb_23, "February 23rd"},
                {Jan_27, "January 27th"},
            }
        });
    }
}