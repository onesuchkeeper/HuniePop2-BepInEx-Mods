using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

[Expansion(typeof(DialogTriggerDefinition), HasModId = true)]
public partial class ExpandedDialogTriggerDefinition
{
    [HarmonyPatch(typeof(DialogTriggerDefinition))]
    private class Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetLineSetByGirl")]
        private static bool GetLineSetByGirl(DialogTriggerDefinition __instance, GirlDefinition girlDef, ref DialogTriggerLineSet __result) 
            => ExpandedDialogTriggerDefinition.Get(__instance).GetLineSetByGirl(girlDef, ref __result);
    }

    private Dictionary<RelativeId, IdIndexMap> _girlToLineIndexes = new();

    /// <summary>
    /// Given a girl, attempts to get their <see cref="DialogTriggerLineSet"/>
    /// </summary>
    public bool TryGetLineSet(DialogTriggerDefinition def, RelativeId girlId, out DialogTriggerLineSet lineSet)
    {
        var girlIndex = ExpandedGirlDefinition.DialogTriggerIndexes[girlId];
        lineSet = def.dialogLineSets.GetOrNew(girlIndex);
        if (lineSet.dialogLines.Any(x => x != null))
        {
            return true;
        }

        lineSet = def.dialogLineSets.FirstOrDefault();
        if (lineSet?.dialogLines.Count > 0)
        {
            return true;
        }

        lineSet = null;
        return false;
    }

    public DialogTriggerLineSet GetLineSetOrNew(DialogTriggerDefinition def, RelativeId girlId)
        => def.dialogLineSets.GetOrNew(ExpandedGirlDefinition.DialogTriggerIndexes[girlId]);

    public DialogLine GetLineOrNew(DialogTriggerDefinition def, RelativeId girlId, RelativeId lineId)
    {
        var set = GetLineSetOrNew(def, girlId);
        var LineIndexes = _girlToLineIndexes.GetOrNew(girlId);
        var index = LineIndexes[lineId];
        return set.dialogLines.GetOrNew(index);
    }

    
    private bool GetLineSetByGirl(GirlDefinition girlDef, ref DialogTriggerLineSet __result)
    {
        try
        {
            var girlExpansion = ExpandedGirlDefinition.Get(girlDef);

            var set = _core.dialogLineSets[ExpandedGirlDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Girl, girlDef.id)]];

            if (set.dialogLines.Any(x => x != null))
            {
                __result = set;
            }
            else if (_core.dialogLineSets[0].dialogLines.Count > 0)
            {
                __result = _core.dialogLineSets[0];
            }
            else
            {
                __result = null;
            }

            return false;
        }
        catch (Exception e)
        {
            ModInterface.Log.Error($"Getting line sets for girl {girlDef.id} - {girlDef.name},", e);
        }

        return true;
    }
}
