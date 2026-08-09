using System.Collections.Generic;
using HarmonyLib;

namespace Hp2BaseMod;

/**
Base Class Mechanics:
 * The base AbilityManager serves as a purely data-driven execution processor for puzzle actions.
 * - Lifecycles: Binds itself globally to `Game.Session.Ability` during `Awake`.
 * - State References: Directly manipulates `UiPuzzleGrid` (to consume, destroy, power, or spawn row/column/type-filtered `UiPuzzleSlot` tokens) 
 *   and `PuzzleStatus` (to modify character resource pools and apply status ailments via `PuzzleStatusGirl`).
 * - Execution Pipeline: Reads sequentially through a static list of `AbilityStepSubDefinition` objects defined on an `AbilityDefinition`. 
 *   It calculates target integers dynamically (e.g., via math constants, random ranges, character-trait metrics, or specific board token counts) 
 *   and operates directly on targeted token subsets using internal conditional matching filters (`TokenCondition`).
 * 
Expansion:
 * This file expands the base data-driven processor by introducing a code-driven, procedural script layer (`IScriptedAbility`) 
 * into the pipeline. The IScriptedAbility is allowed to act before and after regular processing of the ability and may skip the
 * base data-driven pipeline altogether.
 */

[HarmonyPatch(typeof(AbilityManager))]
internal static class AbilityManagerPatch
{
    [HarmonyPatch("PerformAbility")]
    [HarmonyPrefix]
    public static bool PerformAbility_Prefix(
        AbilityManager __instance,
        AbilityDefinition abilityDef,
        bool altGirl,
        Dictionary<string, int> insertValues,
        ref bool __result,
        out (IScriptedAbility Scripted, Ability Ability) __state)
        => ExpandedAbilityManager.Get(__instance).PerformAbility_Prefix(abilityDef, altGirl, insertValues, ref __result, out __state);

    [HarmonyPatch("PerformAbility")]
    [HarmonyPostfix]
    public static void PerformAbility_Postfix(
        AbilityManager __instance,
        AbilityDefinition abilityDef,
        bool altGirl,
        ref bool __result,
        (IScriptedAbility Scripted, Ability Ability) __state)
        => ExpandedAbilityManager.Get(__instance).PerformAbility_Postfix(abilityDef, altGirl, ref __result, __state);
}

[Expansion(typeof(AbilityManager))]
public partial class ExpandedAbilityManager
{
    /// <summary>
    /// Returns false (skip original) if ReplacePerform is provided or PrePerform aborts.
    /// Returns true (run original) for purely data-driven abilities or when PrePerform passes.
    /// </summary>
    internal bool PerformAbility_Prefix(
        AbilityDefinition abilityDef,
        bool altGirl,
        Dictionary<string, int> insertValues,
        ref bool __result, 
        out (IScriptedAbility Scripted, Ability Ability) __state)
    {
        __state = (null, null);

        var factory = abilityDef.GetExpansion().ScriptedAbilityFactory;
        if (factory == null) return true;

        // Mirror Ability construction from the original PerformAbility so the factory
        // receives a fully initialized instance (values populated, definition set).
        var ability = new Ability(abilityDef, altGirl);
        if (insertValues != null)
        {
            ListUtils.DictionaryAddRangeUnique(ability.values, insertValues);
        }

        var scripted = factory(ability);

        __state = (scripted, ability);

        if (!scripted.PrePerform(ability, altGirl))
        {
            __result = false;
            return false;
        }

        var replaced = scripted.ReplacePerform(ability, altGirl);
        if (replaced.HasValue)
        {
            __result = replaced.Value;
            return false;
        }

        // No replacement let the original pipeline run.
        return true;
    }

    /// <summary>
    /// Runs PostPerform using the scripted ability set during the prefix, if any.
    /// </summary>
    internal void PerformAbility_Postfix(AbilityDefinition abilityDef, bool altGirl, ref bool __result, (IScriptedAbility Scripted, Ability Ability) __state)
    {
        if (__state.Scripted == null) return;
        __result = __state.Scripted.PostPerform(__state.Ability, altGirl, __result);
    }
}