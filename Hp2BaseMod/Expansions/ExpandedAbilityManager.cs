using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

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

/// <summary>
/// Companion class for <see cref="AbilityManager"/> that injects <see cref="IScriptedAbility"/> calls
/// into the PerformAbility pipeline.
/// </summary>
public class ExpandedAbilityManager
{
    private static readonly Dictionary<AbilityManager, ExpandedAbilityManager> _expansions
        = new Dictionary<AbilityManager, ExpandedAbilityManager>();

    public static ExpandedAbilityManager Get(AbilityManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedAbilityManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    // PerformAbility constructs Ability internally, so we need to replicate that
    // construction here to attach the scripted behaviour before any hooks fire.
    private static readonly FieldInfo f_abilityQueue = AccessTools.Field(typeof(AbilityManager), "_abilityQueue");

    private readonly AbilityManager _core;
    private ExpandedAbilityManager(AbilityManager core)
    {
        _core = core;
    }

    /// <summary>
    /// Returns false (skip original) if ReplacePerform is provided or PrePerform aborts.
    /// Returns true (run original) for purely data-driven abilities or when PrePerform passes.
    /// </summary>
    public bool PerformAbility_Prefix(
        AbilityDefinition abilityDef,
        bool altGirl,
        Dictionary<string, int> insertValues,
        ref bool __result, 
        out (IScriptedAbility Scripted, Ability Ability) __state)
    {
        __state = (null, null);

        var factory = abilityDef.Expansion().ScriptedAbilityFactory;
        if (factory == null) return true;

        // Mirror Ability construction from the original PerformAbility so the factory
        // receives a fully initialized instance (values populated, definition set).
        var ability = new Ability(abilityDef, altGirl);
        if (insertValues != null)
        {
            ListUtils.DictionaryAddRangeUnique(ability.values, insertValues);
        }

        var scripted = factory(ability);
        ability.Expansion().ScriptedAbility = scripted;

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
    public void PerformAbility_Postfix(AbilityDefinition abilityDef, bool altGirl, ref bool __result, (IScriptedAbility Scripted, Ability Ability) __state)
    {
        if (__state.Scripted == null) return;

        // When the original pipeline ran (no ReplacePerform), _activeAbility is the instance
        // we constructed in the prefix. PostPerform uses it for any per-instance state.
        __result = __state.Scripted.PostPerform(__state.Ability, altGirl, __result);
    }
}