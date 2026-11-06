// using System.Reflection;
// using HarmonyLib;

// namespace SingleDate;

// /// <summary>
// /// Keeps the puzzle's focus on the right girl on single dates. The base game switches focus between the two girls,
// /// but on a single date the right girl is the only one.
// /// </summary>
// [HarmonyPatch(typeof(PuzzleStatus))]
// internal static class PuzzleStatusPatch
// {
//     // Private fields of PuzzleStatus. _altGirlFocused is true when the right (alt) girl is the focused girl.
//     private static readonly FieldInfo f_altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");
//     private static readonly FieldInfo f_affection = AccessTools.Field(typeof(PuzzleStatus), "_affection");
//     private static readonly FieldInfo f_affectionGoal = AccessTools.Field(typeof(PuzzleStatus), "_affectionGoal");

//     /// <summary>After the base game picks focus for the next round, forces it back to the right girl.</summary>
//     [HarmonyPatch(nameof(PuzzleStatus.NextRound))]
//     [HarmonyPostfix]
//     public static void NextRound(PuzzleStatus __instance, int staminaOverride, bool checkStaminaFreeze)
//     {
//         if (!State.IsSingleDate) return;
//         f_altGirlFocused.SetValue(__instance, true);
//     }

//     /// <summary>Any request to focus a girl is changed to focus the right girl.</summary>
//     [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocus))]
//     [HarmonyPrefix]
//     public static void SetGirlFocus(PuzzleStatus __instance, ref bool altGirl)
//     {
//         if (!State.IsSingleDate) return;
//         altGirl = true;
//     }

//     /// <summary>Focus is normally picked by stamina, force it back to the right girl.</summary>
//     [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocusByStamina))]
//     [HarmonyPostfix]
//     public static void SetGirlFocusByStamina(PuzzleStatus __instance)
//     {
//         if (!State.IsSingleDate) return;
//         f_altGirlFocused.SetValue(__instance, true);
//     }
// }
