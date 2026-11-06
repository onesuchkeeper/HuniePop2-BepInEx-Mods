// Hp2BaseModLoader 2021, by OneSuchKeeper

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod.Save
{
    [HarmonyPatch(typeof(GamePersistence))]
    internal static class GamePersistencePatches
    {
        private static readonly FieldInfo f_saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");
        private static readonly FieldInfo f_inited = AccessTools.Field(typeof(GamePersistence), "_inited");
        private static readonly FieldInfo f_debugMode = AccessTools.Field(typeof(GamePersistence), "_debugMode");

        [HarmonyPrefix]
        [HarmonyPatch("Save")]
        private static bool SavePre(GamePersistence __instance, out SaveData __state)
        {
            ModInterface.Events.NotifyPreSave();

            //create copy of save data
            var saveData = (SaveData)f_saveData.GetValue(__instance);
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
                ModInterface.Log.Error("Mod data strip failed", e);
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
                f_saveData.SetValue(__instance, __state);
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
                var saveData = f_saveData.GetValue(__instance) as SaveData;
                // injects the mod data, each file goes through a SavePipeline
                ModInterface.InjectSave(saveData);
                using (ModInterface.Log.MakeIndent("Cleaning Save Data")) CleanSave(saveData);
            }
            catch (Exception e)
            {
                ModInterface.Log.Error("Exception thrown while loading mod save", e);
            }
        }

        /// <summary>
        /// Every path that turns a PlayerFile into a SaveFile for a single slot ends up here. The erase and new game
        /// screens use it, and they only get to the pipeline through this, after the file was written
        /// and before it is saved. Normal in-game applies have nothing pending and are untouched.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GamePersistence.Apply), typeof(int))]
        private static void ApplyPost(int fileIndex)
        {
            ModInterface.SaveManager.OnFileApplied(fileIndex);
        }

        [HarmonyPrefix]
        [HarmonyPatch("Reset")]
        private static void ResetPre(GamePersistence __instance)
        {
            if ((bool)f_inited.GetValue(__instance))
            {
                ModInterface.ApplyDataMods();
                ModInterface.Events.NotifyPrePersistenceReset(f_saveData.GetValue(__instance) as SaveData);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Reset")]
        private static void ResetPost(GamePersistence __instance)
        {
            if ((bool)f_inited.GetValue(__instance))
            {
                ModInterface.Events.NotifyPostPersistenceReset(f_saveData.GetValue(__instance) as SaveData);
                ModInterface.Log.DisplayErrors();
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

        // files are cleaned per file by SavePipeline
        private static void CleanSave(SaveData saveData)
        {
            saveData.unlockedCodes = saveData.unlockedCodes?
                    .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Code, x, out _))
                    .ToList()
                ?? new();

            saveData.files = saveData.files?.Where(x => x != null).ToList() ?? new();
        }
    }
}