// Hp2BaseModLoader 2021, by OneSuchKeeper

using System;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod.Save
{
    [HarmonyPatch(typeof(GamePersistence))]
    internal static class GamePersistencePatches
    {
        private static FieldInfo f_saveDataAccess = AccessTools.Field(typeof(GamePersistence), "_saveData");
        private static FieldInfo f_inited = AccessTools.Field(typeof(GamePersistence), "_inited");
        private static FieldInfo f_debugMode = AccessTools.Field(typeof(GamePersistence), "_debugMode");

        [HarmonyPrefix]
        [HarmonyPatch("Save")]
        private static bool SavePre(GamePersistence __instance, out SaveData __state)
        {
            ModInterface.Events.NotifyPreSave();

            //create copy of save data
            var saveData = (SaveData)f_saveDataAccess.GetValue(__instance);
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
                f_saveDataAccess.SetValue(__instance, __state);
            }

            ModInterface.Events.NotifyPostSave();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Load")]
        private static void LoadPost(GamePersistence __instance)
        {
            if (!(bool)f_inited.GetValue(__instance)
                || (bool)f_debugMode.GetValue(__instance))
            {
                return;
            }

            try
            {
                var saveData = f_saveDataAccess.GetValue(__instance) as SaveData;
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
            if ((bool)f_inited.GetValue(__instance))
            {
                ModInterface.ApplyDataMods();
                ModInterface.Events.NotifyPrePersistenceReset(f_saveDataAccess.GetValue(__instance) as SaveData);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Reset")]
        private static void ResetPost(GamePersistence __instance)
        {
            if ((bool)f_inited.GetValue(__instance))
            {
                ModInterface.Events.NotifyPostPersistenceReset(f_saveDataAccess.GetValue(__instance) as SaveData);
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
            if ((bool)f_inited.GetValue(__instance))
            {
                return;
            }

            ModInterface.ApplyDataMods();
        }
    }
}
