// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using System.Linq;
using HarmonyLib;

namespace Hp2BaseMod.EnumExpansion
{
    /// <summary>
    /// Overrides getting line sets to use relative ids
    /// </summary>
    [HarmonyPatch(typeof(DialogTriggerDefinition))]
    internal class DialogTriggerDefinitionPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetLineSetByGirl")]
        public static bool GetLineSetByGirl(DialogTriggerDefinition __instance, GirlDefinition girlDef, ref DialogTriggerLineSet __result)
        {
            try
            {
                var girlExpansion = ExpandedGirlDefinition.Get(girlDef);

                var set = __instance.dialogLineSets[ExpandedGirlDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Girl, girlDef.id)]];

                if (set.dialogLines.Any(x => x != null))
                {
                    __result = set;
                }
                else if (__instance.dialogLineSets[0].dialogLines.Count > 0)
                {
                    __result = __instance.dialogLineSets[0];
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
}
