using System.Collections.Generic;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

// [HarmonyPatch(typeof(PuzzleStatusGirl))]
// internal static class PuzzleStatusGirlPatch
// {
//     [HarmonyPatch("ApplyAilment")]
//     [HarmonyPostfix]
//     public static void ApplyAilment(PuzzleStatusGirl __instance, AilmentDefinition ailmentDef, bool __result)
//         => ExpandedPuzzleStatusGirl.Get(__instance).ApplyAilment(ailmentDef, __result);

//     [HarmonyPatch("PopulateAilments")]
//     [HarmonyPostfix]
//     public static void PopulateAilments(PuzzleStatusGirl __instance)
//         => ExpandedPuzzleStatusGirl.Get(__instance).PopulateAilments();
// }

/// <summary>
/// Companion class for <see cref="PuzzleStatusGirl"/> that attaches <see cref="IScriptedAilment"/>
/// instances to newly constructed <see cref="Ailment"/> objects when the definition's
/// <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/> is set.
/// </summary>
[Expansion(typeof(PuzzleStatusGirl))]
public partial class ExpandedPuzzleStatusGirl
{
    // /// <summary>
    // /// After ApplyAilment succeeds, attach a scripted behaviour to the new Ailment instance
    // /// if the definition has a factory set.
    // /// </summary>
    // internal void ApplyAilment(AilmentDefinition ailmentDef, bool result)
    // {
    //     if (!result) return;

    //     var factory = ailmentDef.GetExpansion().ScriptedAilmentFactory;
    //     if (factory == null) return;

    //     // The ailment was just added as the last entry by the original method.
    //     var ailments = f_ailments.GetValue<List<Ailment>>(_core);
    //     var ailment = ailments[ailments.Count - 1];
    //     ailment.GetExpansion().ScriptedAilment = factory(ailment);
    // }

    // /// <summary>
    // /// After PopulateAilments runs, attach scripted behaviour to any ailment instances
    // /// whose definitions have a factory set.
    // /// </summary>
    // internal void PopulateAilments()
    // {
    //     var ailments = f_ailments.GetValue<List<Ailment>>(_core);

    //     foreach (var ailment in ailments)
    //     {
    //         var factory = ailment.definition.GetExpansion().ScriptedAilmentFactory;
    //         if (factory == null) continue;

    //         // Only attach if not already populated (guard against double-calls).
    //         var expansion = ailment.GetExpansion();
    //         if (expansion.ScriptedAilment == null)
    //         {
    //             expansion.ScriptedAilment = factory(ailment);
    //         }
    //     }
    // }

    private void OnDestroy()
    {
        foreach (var ailment in _core.ailments)
        {
            ailment.DestroyExpansion();
        }
    }
}