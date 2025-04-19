using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    [HarmonyPatch(nameof(UiDoll.ShowEnergySurge))]
    [HarmonyPrefix]
    public static bool ShowEnergySurge(UiDoll __instance, EnergyDefinition energyDef, float duration, bool knockback, bool negative = false, bool silent = false)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiDoll.ChangeExpression), [typeof(GirlExpressionType), typeof(bool)])]
    [HarmonyPrefix]
    public static bool ChangeExpression(UiDoll __instance, GirlExpressionType expressionType, bool eyesClosed = false)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiDoll.ChangeExpression), [typeof(int), typeof(bool)])]
    [HarmonyPrefix]
    public static bool ChangeExpression(UiDoll __instance, int expressionIndex = -1, bool eyesClosed = false)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiDoll.ChangeOutfit))]
    [HarmonyPrefix]
    public static bool ChangeOutfit(UiDoll __instance, int outfitIndex = -1)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(nameof(UiDoll.ChangeHairstyle))]
    [HarmonyPrefix]
    public static bool ChangeHairstyle(UiDoll __instance, int hairstyleIndex = -1)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }

    // [HarmonyPatch(nameof(UiDoll.ReadDialogLine))]
    // [HarmonyPrefix]
    // public static void ReadDialogLine(UiDoll __instance, DialogLine dialogLine, DialogLineFormat format, int priority = -1)
    // {
    //     if (__instance.girlDefinition == null)
    //     {
    //         throw new System.Exception("Attempt to read line with unpopulated doll");
    //     }
    // }

    // [HarmonyPatch(nameof(UiDoll.ReadDialogTrigger))]
    // [HarmonyPrefix]
    // public static void ReadDialogTrigger(UiDoll __instance, DialogTriggerDefinition dialogTriggerDef, DialogLineFormat format, int lineIndex = -1)
    // {
    //     if (__instance.girlDefinition == null)
    //     {
    //         throw new System.Exception("Attempt to read line with unpopulated doll");
    //     }
    // }
}