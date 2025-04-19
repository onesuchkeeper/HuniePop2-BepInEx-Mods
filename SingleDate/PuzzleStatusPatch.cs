using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleStatus))]
public static class PuzzleStatusPatch
{
    private static readonly FieldInfo _altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    [HarmonyPatch(nameof(PuzzleStatus.Reset), [typeof(List<GirlDefinition>), typeof(bool)])]
    [HarmonyPostfix]
    public static void Reset(PuzzleStatus __instance, List<GirlDefinition> girlList, bool nonstop)
    {
        if (!State.IsLocationPairSingle())
        {
            return;
        }

        _altGirlFocused.SetValue(__instance, true);
    }
}