using System.Collections.Generic;
using HarmonyLib;

namespace Hp2BaseMod;

public static class GirlSpecialPartSubDefinition_Ext
{
    public static ExpandedGirlSpecialPartSubDefinition Expansion(this GirlSpecialPartSubDefinition def)
        => ExpandedGirlSpecialPartSubDefinition.Get(def);
}

// [HarmonyPatch(typeof(GirlSpecialPartSubDefinition))]
// public static class GirlSpecialPartSubDefinitionPatch
// {

//     [HarmonyPatch(nameof(GirlSpecialPartSubDefinition.Method))]
//     [HarmonyPostfix]
//     public static void Method(GirlSpecialPartSubDefinition __instance)
//     {

//     }
// }

public class ExpandedGirlSpecialPartSubDefinition
{
    private static Dictionary<GirlSpecialPartSubDefinition, ExpandedGirlSpecialPartSubDefinition> _expansions
        = new Dictionary<GirlSpecialPartSubDefinition, ExpandedGirlSpecialPartSubDefinition>();

    public static ExpandedGirlSpecialPartSubDefinition Get(GirlSpecialPartSubDefinition core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedGirlSpecialPartSubDefinition(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    protected GirlSpecialPartSubDefinition _core;

    public List<RelativeId> RequiredHairstyles;

    private ExpandedGirlSpecialPartSubDefinition(GirlSpecialPartSubDefinition core)
    {
        _core = core;
    }
}