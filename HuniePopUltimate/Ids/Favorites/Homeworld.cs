using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class Homeworld
{
    public static RelativeId Earth = new RelativeId(Plugin.ModId, 0);
    public static RelativeId Unsure = new RelativeId(Plugin.ModId, 1);
    public static RelativeId Tendricide = new RelativeId(Plugin.ModId, 2);
    public static RelativeId SkyGarden = new RelativeId(Plugin.ModId, 3);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new QuestionDataMod(Questions.HomeWorld, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Homeworld",
            QuestionText = "Where [[highlight]are you from]?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {Earth, "Earth"},
                {Unsure, "Unsure"},
                {Tendricide, "Tendricide"},
                {SkyGarden, "Sky Garden"},
            }
        });
    }
}