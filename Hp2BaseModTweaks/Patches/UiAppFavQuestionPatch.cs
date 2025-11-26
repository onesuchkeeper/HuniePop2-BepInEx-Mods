using System.Reflection;
using HarmonyLib;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppFavAnswer))]
public static class UiAppFavAnswerPatch
{
    private static readonly FieldInfo f_favQuestionDefinition = AccessTools.Field(typeof(UiAppFavAnswer), "_favQuestionDefinition");

    [HarmonyPatch(nameof(UiAppFavAnswer.Populate))]
    [HarmonyPrefix]
    public static bool Populate(UiAppFavAnswer __instance, PlayerFileGirl playerFileGirl, PlayerFileGirl otherFileGirl)
    {
        var favAnswers = playerFileGirl.girlDefinition.favAnswers;

        if (__instance.favAnswerIndex < 0
            || __instance.favAnswerIndex >= favAnswers.Count
            || favAnswers[__instance.favAnswerIndex] == -1)
        {
            //var def = f_favQuestionDefinition.GetValue<QuestionDefinition>(__instance);
            __instance.textLabel.text = string.Empty;//$"<color=#85F7FF40>•{def.questionName}: ???</color>";
            return false;
        }

        return true;
    }
}