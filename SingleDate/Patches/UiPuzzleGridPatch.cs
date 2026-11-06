// using Hp2BaseMod;
// using HarmonyLib;

// namespace SingleDate;
  
// /// <summary>Starts the single date ailment when a single date's puzzle starts.</summary>
// [HarmonyPatch(typeof(UiPuzzleGrid))]
// internal static class SingleDateStartPuzzlePatch
// {
//     /// <summary>Adds <see cref="Ailments.SingleDateAilment"/> as a global ailment (always active for the date) on single dates.</summary>
//     [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
//     [HarmonyPrefix]
//     public static void Prefix(UiPuzzleGrid __instance)
//     {
//         if (!State.IsSingleDate) return;
//         Game.Session.Ailment.GetExpansion().AddGlobal(Ailments.SingleDateAilment);
//     }
// }
