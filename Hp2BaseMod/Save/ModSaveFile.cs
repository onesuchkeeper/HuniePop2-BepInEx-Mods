// Hp2BaseModLoader 2021, by OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.Save;

[Serializable]
public class ModSaveFile
{
    private const string WARDROBE_GIRL_ID_FLAG = "wardrobe_girl_id";
    private const int HOTEL_ROOM_LOC_ID = 21;
    private const int LOLA_GIRL_ID = 1;
    private const int DEFAULT_INV_SLOT_COUNT = 35;
    private const int DEFAULT_SHOP_SLOT_COUNT = 32;
    private const int DEFAULT_GIRL_COUNT = 12;
    private const int DEFAULT_PAIR_COUNT = 24;
    private const int _defaultFruitCount = 4;
    private const int _defaultAffectionCount = 4;

    public RelativeId? WardrobeGirlId;
    public RelativeId? LocationId;
    public RelativeId? GirlPairId;
    public RelativeId? FileIconGirlId;

    public List<RelativeId> MetGirlPairs = new();
    public List<RelativeId> CompletedGirlPairs = new();

    public Dictionary<RelativeId, ModSaveGirl> Girls = new();
    public Dictionary<RelativeId, ModSaveGirlPair> GirlPairs = new();
    public Dictionary<RelativeId, ModSaveFinderSlot> FinderSlots = new();

    public List<ModSaveInventorySlot> InventorySlots = new();
    public List<ModSaveStoreProduct> StoreProducts = new();

    /// <summary>
    /// Copies the save file and strips modded data out of it
    /// </summary>
    /// <param name="saveFile">savefile to strip</param>
    public void Strip(SaveFile saveFile)
    {
        // wardrobe girl id
        var wardrobeGirlIdFlag = saveFile.flags.FirstOrDefault(x => x.flagName == WARDROBE_GIRL_ID_FLAG);
        var wardrobeGirlId = wardrobeGirlIdFlag?.flagValue;
        if (wardrobeGirlId.HasValue)
        {
            WardrobeGirlId = ModInterface.Data.GetDataId(GameDataType.Girl, wardrobeGirlId.Value);

            if (WardrobeGirlId.Value.SourceId != -1)
            {
                wardrobeGirlIdFlag.flagValue = LOLA_GIRL_ID;
            }
        }
        else
        {
            WardrobeGirlId = null;
        }

        // icon
        // if invalid default to lola
        FileIconGirlId = ModInterface.Data.GetDataId(GameDataType.Girl, saveFile.fileIconGirlId);
        if (FileIconGirlId.Value.SourceId != -1)
        {
            saveFile.fileIconGirlId = LOLA_GIRL_ID;
        }

        // current location
        // if current location is invalid, go back to hub
        LocationId = ModInterface.Data.GetDataId(GameDataType.Location, saveFile.locationId);
        if (LocationId.Value.SourceId != -1)
        {
            saveFile.locationId = HOTEL_ROOM_LOC_ID;
        }

        // current pair
        // if the current pair is invalid, go back to the hub
        GirlPairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, saveFile.girlPairId);
        if (GirlPairId.Value.SourceId != -1)
        {
            saveFile.girlPairId = -1;
            saveFile.locationId = HOTEL_ROOM_LOC_ID;
        }

        // if at hub make sure there isn't a current pair
        if (saveFile.locationId == HOTEL_ROOM_LOC_ID)
        {
            saveFile.girlPairId = -1;
        }

        // girls
        // first 
        var newGirls = new Dictionary<RelativeId, ModSaveGirl>();
        foreach (var girl in saveFile.girls)
        {
            var id = ModInterface.Data.GetDataId(GameDataType.Girl, girl.girlId);
            var modSave = Girls.GetOrNew(id);
            modSave.Strip(girl);
            newGirls[id] = modSave;
        }
        Girls = newGirls;
        saveFile.girls = saveFile.girls.Take(DEFAULT_GIRL_COUNT).ToList();

        // pairs
        var newPairs = new Dictionary<RelativeId, ModSaveGirlPair>();
        foreach (var pair in saveFile.girlPairs)
        {
            var id = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.girlPairId);
            var modSave = GirlPairs.GetOrNew(id);
            modSave.Strip(pair);
            newPairs[id] = modSave;
        }
        GirlPairs = newPairs;
        saveFile.girlPairs = saveFile.girlPairs.Take(DEFAULT_PAIR_COUNT).ToList();

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
        var newFinderSlots = new Dictionary<RelativeId, ModSaveFinderSlot>();
        var strippedFinderSlots = new List<SaveFileFinderSlot>();
        foreach (var slot in saveFile.finderSlots)
        {
            var locationId = ModInterface.Data.GetDataId(GameDataType.Location, slot.locationId);
            var modSave = FinderSlots.GetOrNew(locationId);
            modSave.Strip(slot);
            newFinderSlots[locationId] = modSave;

            if (locationId.SourceId == -1)
            {
                strippedFinderSlots.Add(slot);
            }
        }
        FinderSlots = newFinderSlots;
        saveFile.finderSlots = strippedFinderSlots;

        // inventory
        // inventory/product slots are also identified only by index, if a modder wants to add some to a different pool
        // they should use a separate data structure, I won't handle it here
        SaveUtility.MatchListLength(saveFile.inventorySlots, InventorySlots);
        foreach (var (save, mod) in saveFile.inventorySlots.Zip(InventorySlots, (save, mod) => (save, mod)))
        {
            mod.Strip(save);
        }
        saveFile.inventorySlots = saveFile.inventorySlots.Take(DEFAULT_INV_SLOT_COUNT).ToList();

        // store
        SaveUtility.MatchListLength(saveFile.storeProducts, StoreProducts);
        foreach (var (save, mod) in saveFile.storeProducts.Zip(StoreProducts, (save, mod) => (save, mod)))
        {
            mod.Strip(save);
        }
        saveFile.storeProducts = saveFile.storeProducts.Take(DEFAULT_SHOP_SLOT_COUNT).ToList();
    }

    public void SetData(SaveFile saveFile)
    {
        var i = 0;

        var wardrobeFlag = saveFile.flags.FirstOrDefault(x => x.flagName == WARDROBE_GIRL_ID_FLAG);
        if (wardrobeFlag != null)
        {
            ValidatedSet.SetFromRelativeId(ref wardrobeFlag.flagValue, GameDataType.GirlPair, WardrobeGirlId);
        }

        ValidatedSet.SetFromRelativeId(ref saveFile.fileIconGirlId, GameDataType.Girl, FileIconGirlId);
        ValidatedSet.SetFromRelativeId(ref saveFile.locationId, GameDataType.Location, LocationId);
        ValidatedSet.SetFromRelativeId(ref saveFile.girlPairId, GameDataType.GirlPair, GirlPairId);

        ValidatedSet.SetModIds(ref saveFile.metGirlPairs, MetGirlPairs, GameDataType.GirlPair);
        ValidatedSet.SetModIds(ref saveFile.completedGirlPairs, CompletedGirlPairs, GameDataType.GirlPair);

        if (WardrobeGirlId.HasValue
            && ModInterface.Data.TryGetRuntimeDataId(GameDataType.Girl, WardrobeGirlId, out var wardrobeGirlRuntimeId))
        {
            saveFile.flags.Add(new SaveFileFlag(WARDROBE_GIRL_ID_FLAG) { flagValue = wardrobeGirlRuntimeId });
        }

        SaveUtility.HandleModSaves(GameDataType.Girl, Girls, saveFile.girls, saveFile.girls.Select(x => x.girlId), "girl");
        SaveUtility.HandleModSaves(GameDataType.GirlPair, GirlPairs, saveFile.girlPairs, saveFile.girlPairs.Select(x => x.girlPairId), "girl pair");
        SaveUtility.HandleModSaves(GameDataType.Location, FinderSlots, saveFile.finderSlots, saveFile.finderSlots.Select(x => x.locationId), "finder slot");

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
        while (storeProdSaveEnum.MoveNext() && storeProductEnum.MoveNext())//important that save is checked first so inv slot doesn't move forward
        {
            storeProductEnum.Current.SetData(storeProdSaveEnum.Current);
            i++;
        }

        while (storeProductEnum.MoveNext())
        {
            saveFile.storeProducts.Add(storeProductEnum.Current.Convert(i++));
        }
    }

    public ModSaveGirl GetGirl(RelativeId id) => Girls.GetOrNew(id);
}