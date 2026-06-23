using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PuzzleStatusGirl))]
internal static class PuzzleStatusGirlPatch
{
    [HarmonyPatch("ApplyAilment")]
    [HarmonyPostfix]
    public static void ApplyAilment(PuzzleStatusGirl __instance, AilmentDefinition ailmentDef, bool __result)
        => ExpandedPuzzleStatusGirl.Get(__instance).ApplyAilment(ailmentDef, __result);

    [HarmonyPatch("PopulateAilments")]
    [HarmonyPostfix]
    public static void PopulateAilments(PuzzleStatusGirl __instance)
        => ExpandedPuzzleStatusGirl.Get(__instance).PopulateAilments();
}

/// <summary>
/// Companion class for <see cref="PuzzleStatusGirl"/> that attaches <see cref="IScriptedAilment"/>
/// instances to newly constructed <see cref="Ailment"/> objects when the definition's
/// <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/> is set.
/// </summary>
public class ExpandedPuzzleStatusGirl
{
    private static readonly Dictionary<PuzzleStatusGirl, ExpandedPuzzleStatusGirl> _expansions
        = new Dictionary<PuzzleStatusGirl, ExpandedPuzzleStatusGirl>();

    public static ExpandedPuzzleStatusGirl Get(PuzzleStatusGirl core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedPuzzleStatusGirl(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_ailments = AccessTools.Field(typeof(PuzzleStatusGirl), "_ailments");

    private readonly PuzzleStatusGirl _core;

    private ExpandedPuzzleStatusGirl(PuzzleStatusGirl core)
    {
        _core = core;
    }

    /// <summary>
    /// After ApplyAilment succeeds, attach a scripted behaviour to the new Ailment instance
    /// if the definition has a factory set.
    /// </summary>
    public void ApplyAilment(AilmentDefinition ailmentDef, bool result)
    {
        if (!result)
        {
            return;
        }

        var factory = ailmentDef.Expansion().ScriptedAilmentFactory;
        if (factory == null)
        {
            return;
        }

        // The ailment was just added as the last entry by the original method.
        var ailments = f_ailments.GetValue(_core) as List<Ailment>;
        var ailment = ailments[ailments.Count - 1];
        ailment.Expansion().ScriptedAilment = factory(ailment);
    }

    /// <summary>
    /// After PopulateAilments runs, attach scripted behaviour to any ailment instances
    /// whose definitions have a factory set.
    /// </summary>
    public void PopulateAilments()
    {
        var ailments = f_ailments.GetValue(_core) as List<Ailment>;

        for (int i = 0; i < ailments.Count; i++)
        {
            Ailment ailment = ailments[i];
            var factory = ailment.definition.Expansion().ScriptedAilmentFactory;
            if (factory == null)
            {
                continue;
            }

            // Only attach if not already populated (guard against double-calls).
            var expansion = ailment.Expansion();
            if (expansion.ScriptedAilment == null)
            {
                expansion.ScriptedAilment = factory(ailment);
            }
        }
    }
}