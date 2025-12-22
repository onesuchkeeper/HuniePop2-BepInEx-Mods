using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavColour
{
    public static RelativeId Pink = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Green = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Blue = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Red = new RelativeId(Plugin.ModId, 3);
    public static RelativeId Cream = new RelativeId(Plugin.ModId, 4);
    public static RelativeId Purple = new RelativeId(Plugin.ModId, 5);
    public static RelativeId Gold = new RelativeId(Plugin.ModId, 6);
    public static RelativeId Silver = new RelativeId(Plugin.ModId, 7);
    public static RelativeId White = new RelativeId(Plugin.ModId, 8);
    public static RelativeId Orange = new RelativeId(Plugin.ModId, 9);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.FavColour, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Favorite Colour",
            QuestionText = "What is your favorite [[highlight]color]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Pink, "Pink"},
                {Green, "Green"},
                {Blue, "Blue"},
                {Red, "Red"},
                {Cream, "Cream"},
                {Purple, "Purple"},
                {Gold, "Gold"},
                {Silver, "Silver"},
                {White, "White"},
                {Orange, "Orange"},
            }
        });
    }
}