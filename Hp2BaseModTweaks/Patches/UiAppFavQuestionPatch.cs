using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppFavAnswer))]
public static class UiAppFavAnswerPatch
{
    private static readonly FieldInfo f_favQuestionDefinition = AccessTools.Field(typeof(UiAppFavAnswer), "_favQuestionDefinition");

    [HarmonyPatch(nameof(UiAppFavAnswer.Populate))]
    [HarmonyPrefix]
    public static bool Populate(UiAppFavAnswer __instance, PlayerFileGirl playerFileGirl, PlayerFileGirl otherFileGirl)
    {
        // manually populate them, since the original works all dumb and gross
        var questionDef = f_favQuestionDefinition.GetValue<QuestionDefinition>(__instance);
        var questionId = ModInterface.Data.GetDataId(GameDataType.Question, questionDef.id);
        var questionDefExp = ExpandedQuestionDefinition.Get(questionId);
        var girlDef = playerFileGirl.girlDefinition;
        var girlDefExp = girlDef.Expansion();

        var learnedAnswer = playerFileGirl.learnedFavs.Contains(questionDef.id);

        if (!girlDefExp.FavQuestionIdToAnswerId.TryGetValue(questionId, out var girlFavoriteAnswer))
        {
            __instance.textLabel.text = $"<color=#FF0000FF>{questionId} - {questionDef.questionName}</color>";
            return false;
        }

        bool isCommonAnswer = learnedAnswer
            && otherFileGirl != null
            && otherFileGirl.learnedFavs.Contains(questionDef.id)
            ;//&& girlFavoriteAnswer == otherFileGirl.girlDefinition.Expansion().FavQuestionIdToAnswerId[questionId];

        if (isCommonAnswer)
        {
            var otherExp = otherFileGirl.girlDefinition.Expansion();
            //in theory this should always be present since the learned faves contains it
            //need to validate
            if (otherExp.FavQuestionIdToAnswerId.TryGetValue(questionId, out var otherFavAnswer))
            {
                isCommonAnswer = girlFavoriteAnswer == otherFavAnswer;
            }
            else
            {
                isCommonAnswer = false;
            }
        }

        string text = "<color=#85F7FF" + (learnedAnswer ? "FF" : "40") + ">•  ";

        if (Game.Session.Location.currentGirlPair != null
            && Game.Session.Location.currentGirlPair.HasGirlDef(playerFileGirl.girlDefinition))
        {
            text = "";
            __instance.textLabel.color = ColorUtils.ColorAlpha(__instance.textLabel.color, 0f);
            __instance.favStar.color = ColorUtils.ColorAlpha(__instance.favStar.color, 0f);
        }
        else if (learnedAnswer)
        {
            string text2 = questionDefExp.GetAnswer(questionDef, girlFavoriteAnswer);
            if (text2.Contains("/"))
            {
                text2 = text2.Split('/')[(Game.Persistence.playerFile.settingGender == SettingGender.FEMALE) ? 1 : 0];
            }
            text = text + questionDef.questionName + ":</color> " + text2;
            __instance.textLabel.color = ColorUtils.ColorAlpha(__instance.textLabel.color, 1f);
            __instance.favStar.color = ColorUtils.ColorAlpha(__instance.favStar.color, (!isCommonAnswer) ? 0 : 1);
        }
        else
        {
            text += "</color> ";
            __instance.textLabel.color = ColorUtils.ColorAlpha(__instance.textLabel.color, 0.25f);
            __instance.favStar.color = ColorUtils.ColorAlpha(__instance.favStar.color, 0f);
        }
        __instance.textLabel.text = text;

        return false;
    }
}
