﻿using System;
using HarmonyLib;

namespace Hp2BaseMod.EnumExpansion
{
    [HarmonyPatch(typeof(DialogTriggerDefinition))]
    internal class DialogTriggerDefinition_GetLineSetByGirlPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetLineSetByGirl")]
        public static bool GetLineSetByGirl(DialogTriggerDefinition __instance, GirlDefinition girlDef, ref DialogTriggerLineSet __result)
        {
            try
            {
                var girlExpansion = ExpandedGirlDefinition.Get(girlDef);

                var set = __instance.dialogLineSets[girlExpansion.DialogTriggerIndex];

                if (set.dialogLines.Count > 0)
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
                ModInterface.Log.LogError($"Getting line sets for girl {girlDef.id} - {girlDef.name},", e);
            }

            return true;
        }
    }
}
