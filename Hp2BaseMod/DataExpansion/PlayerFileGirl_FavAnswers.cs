using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

/// <summary>
/// Replaces PlayerFileGirl.LearnFavAnswer to use
/// the question's id rather than its index in the
/// Game.Session.Talk.favQuestionDefinitions list
/// </summary>
[HarmonyPatch(typeof(PlayerFileGirl))]
internal static class PlayerFileGirl_FavAnswers
{
    private static readonly FieldInfo f_learnedFavs = AccessTools.Field(typeof(PlayerFileGirl), "_learnedFavs");

    [HarmonyPatch(nameof(PlayerFileGirl.LearnFavAnswer))]
    [HarmonyPrefix]
    public static bool LearnFavAnswer(PlayerFileGirl __instance, QuestionDefinition questionDef, ref bool __result)
    {
        var learnedFavs = f_learnedFavs.GetValue<List<int>>(__instance);
        __result = !learnedFavs.Contains(questionDef.id);

        if (__result)
        {
            learnedFavs.Add(questionDef.id);
        }

        return false;
    }

    [HarmonyPatch(nameof(PlayerFileGirl.HasFavAnswer))]
    [HarmonyPrefix]
    public static bool HasFavAnswer(PlayerFileGirl __instance, QuestionDefinition questionDef, ref bool __result)
    {
        var learnedFavs = f_learnedFavs.GetValue<List<int>>(__instance);
        __result = !learnedFavs.Contains(questionDef.id);
        return false;
    }

    [HarmonyPatch(nameof(PlayerFileGirl.LearnBaggage))]
    [HarmonyPrefix]
    public static void LearnBaggage(PlayerFileGirl __instance, ItemDefinition baggageDef, ref bool __result)
    {
        ModInterface.Log.Message($"Learning Baggage {baggageDef.id} - {baggageDef.itemName}");
    }
}
