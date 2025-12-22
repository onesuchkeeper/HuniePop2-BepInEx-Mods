using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class FavHobby
{
    public static RelativeId Cheerleading = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Gambling = new RelativeId(Plugin.ModId, 1);
    public static RelativeId WorkingOut = new RelativeId(Plugin.ModId, 2);
    public static RelativeId Shopping = new RelativeId(Plugin.ModId, 3);
    public static RelativeId Gaming = new RelativeId(Plugin.ModId, 4);
    public static RelativeId Meditation = new RelativeId(Plugin.ModId, 5);
    public static RelativeId Sleeping = new RelativeId(Plugin.ModId, 6);
    public static RelativeId Swimming = new RelativeId(Plugin.ModId, 7);
    public static RelativeId Porn = new RelativeId(Plugin.ModId, 8);
    public static RelativeId Relaxing = new RelativeId(Plugin.ModId, 9);
    public static RelativeId Tennis = new RelativeId(Plugin.ModId, 10);
    public static RelativeId Drinking = new RelativeId(Plugin.ModId, 11);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.Hobby, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Hobby",
            QuestionText = "What is [[highlight]your hobby]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Cheerleading, "Cheerleading"},
                {Gambling, "Gambling"},
                {WorkingOut, "Working out"},
                {Shopping, "Shopping"},
                {Gaming, "Gaming"},
                {Meditation, "Meditation"},
                {Sleeping, "Sleeping"},
                {Swimming, "Swimming"},
                {Porn, "Watching Porn"},
                {Relaxing, "Relaxing"},
                {Tennis, "Playing Tennis"},
                {Drinking, "Drinking"},
            }
        });
    }
}
