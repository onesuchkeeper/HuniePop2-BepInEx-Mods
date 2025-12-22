using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public static class CupSize
{
    public static RelativeId C_Cup = new RelativeId(Plugin.ModId, 0);
    public static RelativeId D_Cup = new RelativeId(Plugin.ModId, 1);
    public static RelativeId DD_Cup = new RelativeId(Plugin.ModId, 2);
    public static RelativeId B_Cup = new RelativeId(Plugin.ModId, 3);
    public static RelativeId E_Cup = new RelativeId(Plugin.ModId, 4);
    public static RelativeId G_Cup = new RelativeId(Plugin.ModId, 5);

    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new FavQuestionDataMod(Questions.CupSize, Hp2BaseMod.Utility.InsertStyle.append)
        {
            QuestionName = "Cup Size",
            QuestionText = "What [[highlight]cup size] you rockin'?",
            QuestionAnswers = new Dictionary<RelativeId, string>()
            {
                {C_Cup, "C"},
                {D_Cup, "D"},
                {DD_Cup, "DD"},
                {B_Cup, "B"},
                {E_Cup, "E"},
                {G_Cup, "G"},
            }
        });
    }
}