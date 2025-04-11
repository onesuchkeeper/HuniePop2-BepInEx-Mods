// Hp2BaseModLoader 2021, by OneSuchKeeper

using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public class ModSaveFile
    {
        private const string _wardrobeGirlIdFlagName = "wardrobe_girl_id";
        private const int _hotelRoomLocationID = 21;
        private const int _lolaGirlID = 1;
        private const int _defaultInventorySlotCount = 35;
        private const int _defaultShopSlotCount = 32;
        private const int _defaultGirlCount = 12;
        private const int _defaultPairCount = 24;

        public bool Started;
        public int SettingGender;
        public int SettingDifficulty;
        public int StoryProgress;
        public int DayTimeElapsed;
        public int FinderRestockTime;
        public int StoreRestockDay;
        public int StaminaFoodLimit;
        public int RelationshipPoints;
        public int AlphaDateCount;
        public int NonstopDataCount;
        public bool SidesFlipped;

        public RelativeId? WardrobeGirlId;
        public RelativeId? LocationId;
        public RelativeId? GirlPairId;
        public RelativeId? FileIconGirlId;

        public List<RelativeId> MetGirlPairs;
        public List<RelativeId> CompletedGirlPairs;

        public Dictionary<RelativeId, int> FruitCounts;
        public Dictionary<RelativeId, int> AffectionLevelExps;

        public Dictionary<RelativeId, ModSaveGirl> Girls;
        public Dictionary<RelativeId, ModSaveGirlPair> GirlPairs;
        public Dictionary<RelativeId, ModSaveFinderSlot> FinderSlots;

        public List<ModSaveInventorySlot> InventorySlots;
        public List<ModSaveStoreProduct> StoreProducts;

        public Dictionary<string, int> Flags;

        private bool _isValid = true;

        /// <summary>
        /// Copies the save file and strips modded data out of it
        /// </summary>
        /// <param name="saveFile">savefile to strip</param>
        public void Strip(SaveFile saveFile)
        {
            Started = saveFile.started;
            SettingGender = saveFile.settingGender;
            SettingDifficulty = saveFile.settingDifficulty;
            StoryProgress = saveFile.storyProgress;
            DayTimeElapsed = saveFile.daytimeElapsed;
            FinderRestockTime = saveFile.finderRestockTime;
            StoreRestockDay = saveFile.storeRestockDay;
            StaminaFoodLimit = saveFile.staminaFoodLimit;
            RelationshipPoints = saveFile.relationshipPoints;
            AlphaDateCount = saveFile.alphaDateCount;
            NonstopDataCount = saveFile.nonstopDateCount;
            SidesFlipped = saveFile.sidesFlipped;

            FruitCounts = new Dictionary<RelativeId, int>();
            CommonSaveMethods.HandleIndex(saveFile.fruitCounts,
                FruitCounts,
                ModInterface.Data.TryGetFruitId);

            saveFile.fruitCounts = new int[]{
                FruitCounts.TryGetValue(AffectionTypes.Talent, out var talentFC) ? talentFC : 0,
                FruitCounts.TryGetValue(AffectionTypes.Flirtation, out var flirtationFC) ? flirtationFC : 0,
                FruitCounts.TryGetValue(AffectionTypes.Romance, out var romanceFC) ? romanceFC : 0,
                FruitCounts.TryGetValue(AffectionTypes.Sexuality, out var sexualityFC) ? sexualityFC : 0,
            };

            AffectionLevelExps = new();
            CommonSaveMethods.HandleIndex(saveFile.affectionLevelExps,
                AffectionLevelExps,
                ModInterface.Data.TryGetAffectionId);

            saveFile.affectionLevelExps = new int[]{
                AffectionLevelExps.TryGetValue(AffectionTypes.Talent, out var talentALE) ? talentALE : 0,
                AffectionLevelExps.TryGetValue(AffectionTypes.Flirtation, out var flirtationALE) ? flirtationALE : 0,
                AffectionLevelExps.TryGetValue(AffectionTypes.Romance, out var romanceALE) ? romanceALE : 0,
                AffectionLevelExps.TryGetValue(AffectionTypes.Sexuality, out var sexualityALE) ? sexualityALE : 0,
            };

            var wardrobeGirlIdFlag = saveFile.flags.FirstOrDefault(x => x.flagName == _wardrobeGirlIdFlagName);
            var wardrobeGirlId = wardrobeGirlIdFlag?.flagValue;
            if (wardrobeGirlId.HasValue)
            {
                WardrobeGirlId = ModInterface.Data.GetDataId(GameDataType.Girl, wardrobeGirlId.Value);

                if (WardrobeGirlId.Value.SourceId != -1)
                {
                    wardrobeGirlIdFlag.flagValue = _lolaGirlID;
                }
            }
            else
            {
                WardrobeGirlId = null;
            }

            // flag "notification_item_id" has an error catch, so it's fine if it has a bad value
            // and I don't think it's used right after a save anyways and it doesn't really seem like it should even belong in the save file
            // no other flags should matter on load, and having unnecessary mod flags in there won't hurt anything,
            // although it's not recommended to use mod flags, there's no way to validate their source. maybe add a place for mod flags in the interface

            Flags = saveFile.flags.GroupBy(x => x.flagName).Select(x => x.First()).ToDictionary(x => x.flagName, x => x.flagValue);

            // icon
            FileIconGirlId = ModInterface.Data.GetDataId(GameDataType.Girl, saveFile.fileIconGirlId);
            if (FileIconGirlId.Value.SourceId != -1)
            {
                saveFile.fileIconGirlId = _lolaGirlID;
            }

            // current location
            LocationId = ModInterface.Data.GetDataId(GameDataType.Location, saveFile.locationId);
            if (LocationId.Value.SourceId != -1)
            {
                saveFile.locationId = _hotelRoomLocationID;
            }

            // current pair
            GirlPairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, saveFile.girlPairId);
            if (GirlPairId.Value.SourceId != -1)
            {
                // if the current pair is invalid, go back to the hub
                saveFile.girlPairId = -1;
                saveFile.locationId = _hotelRoomLocationID;
            }

            // Girls
            Girls = new Dictionary<RelativeId, ModSaveGirl>();
            foreach (var girl in saveFile.girls)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Girl, girl.girlId);
                var modSave = new ModSaveGirl();
                modSave.Strip(girl);
                Girls[id] = modSave;
            }

            saveFile.girls = saveFile.girls.Take(_defaultGirlCount).ToList();

            // Pairs
            GirlPairs = new Dictionary<RelativeId, ModSaveGirlPair>();
            foreach (var pair in saveFile.girlPairs)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.girlPairId);
                var modSave = new ModSaveGirlPair();
                modSave.Strip(pair);
                GirlPairs[id] = modSave;
            }

            saveFile.girlPairs = saveFile.girlPairs.Take(_defaultPairCount).ToList();


            //why are met and complete stored here and also in the pairs themselves?

            // met pairs
            MetGirlPairs = saveFile.metGirlPairs
                .Select(x => ModInterface.Data.GetDataId(GameDataType.GirlPair, x))
                .ToList();

            saveFile.metGirlPairs = MetGirlPairs.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            // complete pairs
            CompletedGirlPairs = saveFile.completedGirlPairs
                .Select(x => ModInterface.Data.GetDataId(GameDataType.GirlPair, x))
                .ToList();

            saveFile.completedGirlPairs = CompletedGirlPairs.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            // finder slots
            // finder slots are stored really oddly, they correspond with unique location ids but are 
            // also identified by index. I'll handle it by location for now but this could need to be changed
            FinderSlots = new Dictionary<RelativeId, ModSaveFinderSlot>();
            var strippedFinderSlots = new List<SaveFileFinderSlot>();
            foreach (var slot in saveFile.finderSlots)
            {
                var locationId = ModInterface.Data.GetDataId(GameDataType.Location, slot.locationId);
                var modSave = new ModSaveFinderSlot();
                modSave.Strip(slot);
                FinderSlots[locationId] = modSave;

                if (locationId.SourceId == -1)
                {
                    strippedFinderSlots.Add(slot);
                }
            }
            saveFile.finderSlots = strippedFinderSlots;

            // inventory
            // inventory/product slots are also identified only by index, if a modder wants to add some to a different pool
            // they should use a separate data structure, I won't handle it here
            InventorySlots = new List<ModSaveInventorySlot>();
            foreach (var slot in saveFile.inventorySlots)
            {
                var modInvSlot = new ModSaveInventorySlot();
                modInvSlot.Strip(slot);
                InventorySlots.Add(modInvSlot);
            }
            saveFile.inventorySlots = saveFile.inventorySlots.Take(_defaultInventorySlotCount).ToList();

            // store
            StoreProducts = new List<ModSaveStoreProduct>();
            foreach (var slot in saveFile.storeProducts)
            {
                var modInvSlot = new ModSaveStoreProduct();
                modInvSlot.Strip(slot);
                StoreProducts.Add(modInvSlot);
            }
            saveFile.storeProducts = saveFile.storeProducts.Take(_defaultShopSlotCount).ToList();

            _isValid = true;
        }

        public void SetData(SaveFile saveFile)
        {
            if (!_isValid)
            {
                throw new Exception("Attempted to set data from invalid save");
            }
            _isValid = false;
            var i = 0;

            var wardrobeFlag = saveFile.flags.FirstOrDefault(x => x.flagName == _wardrobeGirlIdFlagName);
            if (wardrobeFlag != null)
            {
                ValidatedSet.SetFromRelativeId(ref wardrobeFlag.flagValue, GameDataType.GirlPair, WardrobeGirlId);
            }

            ValidatedSet.SetFromRelativeId(ref saveFile.fileIconGirlId, GameDataType.Girl, FileIconGirlId);
            ValidatedSet.SetFromRelativeId(ref saveFile.locationId, GameDataType.Location, LocationId);
            ValidatedSet.SetFromRelativeId(ref saveFile.girlPairId, GameDataType.GirlPair, GirlPairId);

            var fruitCounts = saveFile.fruitCounts;
            saveFile.fruitCounts = new int[ModInterface.Data.MaxFruitIndex + 1];
            for (i = 0; i < fruitCounts.Length; i++)
            {
                saveFile.fruitCounts[i] = fruitCounts[i];

                if (ModInterface.Data.TryGetFruitId(i, out var id))
                {
                    FruitCounts.Remove(id);
                }
            }

            var affectionLevelExps = saveFile.affectionLevelExps;
            saveFile.affectionLevelExps = new int[ModInterface.Data.MaxAffectionIndex + 1];
            for (i = 0; i < affectionLevelExps.Length; i++)
            {
                saveFile.affectionLevelExps[i] = affectionLevelExps[i];

                if (ModInterface.Data.TryGetAffectionId(i, out var id))
                {
                    AffectionLevelExps.Remove(id);
                }
            }

            foreach (var girl in saveFile.girls)
            {
                if (ModInterface.Data.TryGetDataId(GameDataType.Girl, girl.girlId, out var girlId)
                    && Girls.TryGetValue(girlId, out var girlMod))
                {
                    girlMod.SetData(girl);
                    Girls.Remove(girlId);
                }
            }

            foreach (var girlPair in saveFile.girlPairs)
            {
                if (ModInterface.Data.TryGetDataId(GameDataType.GirlPair, girlPair.girlPairId, out var girlPairId)
                    && GirlPairs.TryGetValue(girlPairId, out var girlPairMod))
                {
                    girlPairMod.SetData(girlPair);
                    GirlPairs.Remove(girlPairId);
                }
            }

            foreach (var slot in saveFile.finderSlots)
            {
                if (ModInterface.Data.TryGetDataId(GameDataType.Location, slot.locationId, out var locationId)
                    && FinderSlots.TryGetValue(locationId, out var finderSlotMod))
                {
                    finderSlotMod.SetData(slot);
                    FinderSlots.Remove(locationId);
                }
            }

            i = 0;
            var invSlotEnum = InventorySlots.GetEnumerator();
            var invSaveEnum = saveFile.inventorySlots.GetEnumerator();
            while (invSaveEnum.MoveNext() && invSlotEnum.MoveNext())//important that save is checked first so inv slot doesn't move forward
            {
                invSlotEnum.Current.SetData(invSaveEnum.Current);
                i++;
            }

            while (invSlotEnum.MoveNext())
            {
                saveFile.inventorySlots.Add(invSlotEnum.Current.Convert(i++));
            }

            i = 0;
            var storeProductEnum = StoreProducts.GetEnumerator();
            var storeProdSaveEnum = saveFile.storeProducts.GetEnumerator();
            while (storeProdSaveEnum.MoveNext() && invSlotEnum.MoveNext())//important that save is checked first so inv slot doesn't move forward
            {
                storeProductEnum.Current.SetData(storeProdSaveEnum.Current);
                i++;
            }

            while (storeProductEnum.MoveNext())
            {
                saveFile.storeProducts.Add(storeProductEnum.Current.Convert(i++));
            }

            Inject(saveFile);
        }

        public SaveFile Convert()
        {
            if (!_isValid)
            {
                throw new Exception("Attempted to set data from invalid save");
            }
            _isValid = false;

            var saveFile = new SaveFile()
            {
                started = Started,
                fileIconGirlId = ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, GirlPairId) ?? _lolaGirlID,
                settingGender = SettingGender,
                settingDifficulty = SettingDifficulty,
                storyProgress = StoryProgress,
                daytimeElapsed = DayTimeElapsed,
                finderRestockTime = FinderRestockTime,
                storeRestockDay = StoreRestockDay,
                staminaFoodLimit = StaminaFoodLimit,
                fruitCounts = new int[ModInterface.Data.MaxFruitIndex + 1],
                affectionLevelExps = new int[ModInterface.Data.MaxAffectionIndex + 1],
                relationshipPoints = RelationshipPoints,
                alphaDateCount = AlphaDateCount,
                nonstopDateCount = NonstopDataCount,
                //locationId = file.locationId,
                //girlPairId = file.girlPairId,
                sidesFlipped = SidesFlipped,
                girls = new List<SaveFileGirl>(),
                girlPairs = new List<SaveFileGirlPair>(),
                metGirlPairs = new List<int>(),
                completedGirlPairs = new List<int>(),
                finderSlots = new List<SaveFileFinderSlot>(),
                inventorySlots = new List<SaveFileInventorySlot>(),
                storeProducts = new List<SaveFileStoreProduct>(),
                flags = new List<SaveFileFlag>()
            };

            var i = saveFile.inventorySlots.Count;
            foreach (var slot in InventorySlots)
            {
                saveFile.inventorySlots.Add(slot.Convert(i++));
            }

            i = saveFile.storeProducts.Count;
            foreach (var slot in StoreProducts)
            {
                saveFile.storeProducts.Add(slot.Convert(i++));
            }

            Inject(saveFile);
            return saveFile;
        }

        private void Inject(SaveFile saveFile)
        {
            foreach (var fruitCount in FruitCounts)
            {
                if (ModInterface.Data.TryGetFruitIndex(fruitCount.Key, out var index))
                {
                    saveFile.fruitCounts[index] = fruitCount.Value;
                }
            }

            foreach (var affectionLevelExp in AffectionLevelExps)
            {
                if (ModInterface.Data.TryGetAffectionIndex(affectionLevelExp.Key, out var index))
                {
                    saveFile.affectionLevelExps[index] = affectionLevelExp.Value;
                }
            }

            if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Location, LocationId, out var locationRuntimeId))
            {
                saveFile.locationId = locationRuntimeId;
            }
            else
            {
                saveFile.locationId = _hotelRoomLocationID;
            }

            if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.GirlPair, GirlPairId, out var girlPairRuntimeId))
            {
                saveFile.girlPairId = girlPairRuntimeId;
            }
            else
            {
                saveFile.locationId = _hotelRoomLocationID;
                saveFile.girlPairId = -1;
            }

            foreach (var girl in Girls)
            {
                if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Girl, girl.Key, out var runtimeId))
                {
                    saveFile.girls.Add(girl.Value.Convert(runtimeId));
                }
            }

            foreach (var girlPair in GirlPairs)
            {
                if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.GirlPair, girlPair.Key, out var runtimeId))
                {
                    saveFile.girlPairs.Add(girlPair.Value.Convert(runtimeId));
                }
            }

            ValidatedSet.SetModIds(ref saveFile.metGirlPairs, MetGirlPairs, GameDataType.GirlPair);
            ValidatedSet.SetModIds(ref saveFile.completedGirlPairs, CompletedGirlPairs, GameDataType.GirlPair);

            //yeah...lets handle them like they're arrays...because there could be two slots with the same location
            // ... so I need to refactor more shit ahhh
            //go back and do that later, I don't wanna now TODO
            foreach (var slot in FinderSlots)
            {
                if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Location, slot.Key, out var runtimeId))
                {
                    saveFile.finderSlots.Add(slot.Value.Convert(runtimeId));
                }
            }

            if (WardrobeGirlId.HasValue
                && ModInterface.Data.TryGetRuntimeDataId(GameDataType.Girl, WardrobeGirlId, out var wardrobeGirlRuntimeId))
            {
                saveFile.flags.Add(new SaveFileFlag(_wardrobeGirlIdFlagName) { flagValue = wardrobeGirlRuntimeId });
            }

            saveFile.flags.AddRange(Flags.Select(x => new SaveFileFlag(x.Key) { flagValue = x.Value }));
        }
    }
}
