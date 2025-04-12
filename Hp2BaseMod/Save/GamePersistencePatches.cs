// Hp2BaseModLoader 2021, by OneSuchKeeper

using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseMod.Save
{
    [HarmonyPatch(typeof(GamePersistence))]
    internal static class GamePersistencePatches
    {
        private static FieldInfo _saveDataAccess = AccessTools.Field(typeof(GamePersistence), "_saveData");
        private static FieldInfo inited = AccessTools.Field(typeof(GamePersistence), "_inited");
        private static FieldInfo debugMode = AccessTools.Field(typeof(GamePersistence), "_debugMode");

        [HarmonyPrefix]
        [HarmonyPatch("Save")]
        private static bool SavePre(GamePersistence __instance, out SaveData __state)
        {
            ModInterface.Events.NotifyPreSave();

            //create copy of save data
            var saveData = (SaveData)_saveDataAccess.GetValue(__instance);
            if (saveData == null)
            {
                __state = null;
                return true;
            }
            __state = saveData.Copy();

            try
            {
                ModInterface.StripSave(saveData);
            }
            catch (Exception e)
            {
                //don't do the normal save if the strip crashes, we don't want to corrupt the normal save
                ModInterface.Log.LogError("Mod data strip failed", e);
                __state = null;
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Save")]
        private static void SavePost(GamePersistence __instance, SaveData __state)
        {
            if (__state != null)
            {
                _saveDataAccess.SetValue(__instance, __state);
            }

            ModInterface.Events.NotifyPostSave();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Load")]
        private static void LoadPost(GamePersistence __instance)
        {
            if (!(bool)inited.GetValue(__instance)
                || (bool)debugMode.GetValue(__instance))
            {
                return;
            }

            try
            {
                var saveData = _saveDataAccess.GetValue(__instance) as SaveData;
                ModInterface.InjectSave(saveData);
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError("Exception thrown while loading mod save", e);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch("Reset")]
        private static void ResetPre(GamePersistence __instance)
        {
            if ((bool)inited.GetValue(__instance))
            {
                ModInterface.ApplyDataMods();
                ModInterface.Events.NotifyPrePersistenceReset(_saveDataAccess.GetValue(__instance) as SaveData);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Reset")]
        private static void ResetPost(GamePersistence __instance)
        {
            if ((bool)inited.GetValue(__instance))
            {
                ModInterface.Events.NotifyPostPersistenceReset();
            }
        }

        /// <summary>
        /// The game can reset without initializing, but if it initializes the mods have to load first
        /// so we handle it in both cases and whichever happens first gets to do it
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        [HarmonyPatch("Init")]
        private static void InitPre(GamePersistence __instance)
        {
            if ((bool)inited.GetValue(__instance))
            {
                return;
            }

            ModInterface.ApplyDataMods();
        }
    }
}
