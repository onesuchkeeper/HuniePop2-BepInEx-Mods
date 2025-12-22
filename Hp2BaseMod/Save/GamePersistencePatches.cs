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
                ModInterface.InjectSave(saveData);
                using (ModInterface.Log.MakeIndent("Cleaning Save Data")) CleanSave(saveData);
            }
            catch (Exception e)
            {
                ModInterface.Log.Error("Exception thrown while loading mod save", e);
            }
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

        private static void CleanSave(SaveData saveData)
        {
            saveData.unlockedCodes = saveData.unlockedCodes?
                    .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Code, x, out _))
                    .ToList()
                ?? new();

            saveData.files = saveData.files?.Where(x => x != null).ToList() ?? new();
            foreach (var (file, modFile) in saveData.files.Zip(ModInterface.Save.ModFiles, (a, b) => (a, b)))
            {
                CleanFile(file, modFile);
            }
        }

        private static void CleanFile(SaveFile saveFile, ModSaveFile modSaveFile)
        {
            var wardrobeFlag = saveFile.flags.FirstOrDefault(x => x.flagName == Flags.WARDROBE_GIRL_ID);
            if (wardrobeFlag != null && !ModInterface.Data.TryGetDataId(GameDataType.Girl, wardrobeFlag.flagValue, out _))
            {
                wardrobeFlag.flagValue = Girls.AshleyId.LocalId;
            }

            if (!ModInterface.Data.TryGetDataId(GameDataType.Girl, saveFile.fileIconGirlId, out _))
            {
                saveFile.fileIconGirlId = Girls.KyuId.LocalId;
            }

            if (!ModInterface.Data.TryGetDataId(GameDataType.GirlPair, saveFile.girlPairId, out _)
                || !ModInterface.Data.TryGetDataId(GameDataType.Location, saveFile.locationId, out _))
            {
                saveFile.locationId = Locations.HotelRoom.LocalId;
                saveFile.girlPairId = -1;
            }

            saveFile.girls = saveFile.girls?.Where(x => TryCleanGirl(x, modSaveFile)).ToList() ?? new();
            saveFile.girlPairs = saveFile.girlPairs?.Where(TryCleanPair).ToList() ?? new();
            saveFile.finderSlots = saveFile.finderSlots?.Where(TryCleanFinderSlot).ToList() ?? new();
            saveFile.inventorySlots = saveFile.inventorySlots?.Where(TryCleanInvSlot).ToList() ?? new();
            saveFile.storeProducts = saveFile.storeProducts?.Where(TryCleanStoreProduct).ToList() ?? new();
        }

        private static bool TryCleanGirl(SaveFileGirl saveGirl, ModSaveFile modSaveFile)
        {
            if (saveGirl == null) return false;
            if (!ModInterface.Data.TryGetDataId(GameDataType.Girl, saveGirl.girlId, out var girlId)) return false;

            var def = Game.Data.Girls.Get(saveGirl.girlId);
            var expansion = def.Expansion();
            var modSaveGirl = modSaveFile.Girls.GetOrNew(girlId);

            if (!expansion.Bodies.TryGetValue(modSaveGirl.BodyId, out var body))
            {
                if (!expansion.Bodies.TryGetFirst(out var firstBody)) throw new Exception($"Girl {girlId} lacks a body");
                body = firstBody.Value;
                modSaveGirl.BodyId = firstBody.Key;
            }

            if ((!IsIndexInCollection(saveGirl.hairstyleIndex, body.Hairstyles.Count))
                || body.Hairstyles[saveGirl.hairstyleIndex] == null)
            {
                saveGirl.hairstyleIndex = body.DefaultHairstyleIndex;
            }

            if ((!IsIndexInCollection(saveGirl.outfitIndex, body.Outfits.Count))
                || body.Outfits[saveGirl.outfitIndex] == null)
            {
                saveGirl.outfitIndex = body.DefaultOutfitIndex;
            }

            saveGirl.unlockedOutfits = saveGirl.unlockedOutfits
                .Where(x => x >= 0 && x < body.Outfits.Count)
                .Distinct()
                .ToList();

            saveGirl.unlockedHairstyles = saveGirl.unlockedHairstyles
                .Where(x => x >= 0 && x < body.Hairstyles.Count)
                .Distinct()
                .ToList();

            saveGirl.learnedBaggage = saveGirl.learnedBaggage
                .Where(x => IsIndexInCollection(x, def.baggageItemDefs.Count))
                .Distinct()
                .ToList();
            if (saveGirl.activeBaggageIndex >= saveGirl.learnedBaggage.Count) saveGirl.activeBaggageIndex = 0;

            saveGirl.receivedUniques = saveGirl.receivedUniques
                .Where(x => IsIndexInCollection(x, def.uniqueItemDefs.Count))
                .Distinct()
                .ToList();
            saveGirl.receivedShoes = saveGirl.receivedShoes
                .Where(x => IsIndexInCollection(x, def.shoesItemDefs.Count))
                .Distinct()
                .ToList();

            saveGirl.learnedFavs = Game.Data.Questions.GetAll().Select(x => x.id).Where(x => saveGirl.learnedFavs.Contains(x)).ToList();

            saveGirl.recentHerQuestions = saveGirl.recentHerQuestions
                .Where(x => IsIndexInCollection(x, def.herQuestions.Count))
                .Distinct()
                .ToList();

            saveGirl.dateGiftSlots = saveGirl.dateGiftSlots.Where(TryCleanInvSlot).ToList();

            return true;
        }

        private static bool TryCleanPair(SaveFileGirlPair pair)
        {
            if (pair == null) return false;

            pair.learnedFavs = pair.learnedFavs
                .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Question, x, out _))
                .Distinct()
                .ToList();

            pair.recentFavQuestions = pair.learnedFavs
                .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Question, x, out _))
                .Distinct()
                .ToList();

            return true;
        }

        private static bool TryCleanStoreProduct(SaveFileStoreProduct product)
        {
            if (product == null) return false;

            if (!ModInterface.Data.TryGetDataId(GameDataType.Item, product.itemId, out _))
            {
                product.itemId = -1;
            }

            return true;
        }

        private static bool TryCleanInvSlot(SaveFileInventorySlot invSlot)
        {
            if (invSlot == null) return false;

            if (!ModInterface.Data.TryGetDataId(GameDataType.Item, invSlot.itemId, out _))
            {
                invSlot.itemId = -1;
            }

            return true;
        }

        private static bool TryCleanFinderSlot(SaveFileFinderSlot finderSlot)
        {
            if (finderSlot == null) return false;
            if (!ModInterface.Data.TryGetDataId(GameDataType.Location, finderSlot.locationId, out _)) return false;

            if (!ModInterface.Data.TryGetDataId(GameDataType.GirlPair, finderSlot.girlPairId, out _))
            {
                finderSlot.girlPairId = -1;
            }

            return true;
        }

        private static bool IsIndexInCollection(int index, int count)
            => (count > 0) && (index >= 0) && (index < count);
    }
}
