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
    public IGameStatePayload GameStatePayload;
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
        if (!saveFile.started)
        {
            GameStatePayload = new GameStateSpecial.Payload();
        }
        else if (ModInterface.GameState.CurrentState is GameStateTitle)
        {
            // There is no session at the title, so no state was played since the payload was set. A new game is saved
            // from here with the special state its pipeline gave it, which the session then begins in.
        }
        else if (ModInterface.GameState.CurrentState.IsSavable)
        {
            GameStatePayload = ModInterface.GameState.CurrentState.GeneratePayload();
        }
        else
        {
            // A state that can't be loaded back into (a date, an event that must be played through) has no payload. Keeping the last
            // one would pair it with a location and pair it was never saved with, so the save is of the hub.
            GameStatePayload = new GameStateHub.Payload();
            saveFile.locationId = HOTEL_ROOM_LOC_ID;
            saveFile.girlPairId = -1;
        }

        // wardrobe girl id
        var wardrobeGirlIdFlag = saveFile.flags.FirstOrDefault(x => x.flagName == Flags.WARDROBE_GIRL_ID);
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

    private IGameStateFactory DefaultToHub(SaveFile saveFile, string reason)
    {
        ModInterface.Log.Warning($"{reason}, defaulting to hub");

        if (!ModInterface.GameData.GameStates.TryGetFactory(Hp2BaseMod.GameStateId.Hub, out var gameState))
        {
            throw new Exception($"Cannot default to hub, no \"hub\" game state found!");
        }

        LocationId = Hp2BaseMod.LocationId.HotelRoom;
        GirlPairId = null;

        GameStatePayload = new GameStateHub.Payload();

        saveFile.girlPairId = -1;
        saveFile.locationId = HOTEL_ROOM_LOC_ID;
        return gameState;
    }

    private IGameStateFactory DetermineGamestate(SaveFile saveFile)
    {
        if (!saveFile.started)
        {
            ModInterface.Log.Message("DEBUG, new save file should be starting in special state!");
            if (!ModInterface.GameData.GameStates.TryGetFactory(GameStateId.Special, out var specialGameState))
            {
                throw new Exception($"Cannot start new save, no \"Special\" game state found!");
            }

            GameStatePayload = new GameStateSpecial.Payload(true);
            return specialGameState;
        }

        if (GameStatePayload == null)
        {
            return DefaultToHub(saveFile, "No game state defined");
        }

        var payloadType = GameStatePayload.GetType();
        if (!ModInterface.GameData.GameStates.TryGetFactory(payloadType, out var gameState))
        {
            return DefaultToHub(saveFile, $"Game state for payload type {payloadType.FullName} not present in the runtime");
        }

        return gameState;
    }

    /// <summary>
    /// Writes the mod data into the save file. Does not validate the result, <see cref="SavePipeline"/> runs
    /// the cleaning and <see cref="ResolveGameState"/> after this.
    /// </summary>
    public void SetData(SaveFile saveFile)
    {
        var wardrobeFlag = saveFile.flags.FirstOrDefault(x => x.flagName == Flags.WARDROBE_GIRL_ID);

        if (WardrobeGirlId.HasValue &&
            ModInterface.Data.TryGetRuntimeDataId(GameDataType.Girl, WardrobeGirlId, out var runtimeId))
        {
            if (wardrobeFlag == null)
            {
                wardrobeFlag = new SaveFileFlag(Flags.WARDROBE_GIRL_ID);
                saveFile.flags.Add(wardrobeFlag);
            }

            wardrobeFlag.flagValue = runtimeId;
        }

        ValidatedSet.TrySetFromRelativeId(ref saveFile.fileIconGirlId, GameDataType.Girl, FileIconGirlId);
        ValidatedSet.TrySetFromRelativeId(ref saveFile.locationId, GameDataType.Location, LocationId);
        ValidatedSet.TrySetFromRelativeId(ref saveFile.girlPairId, GameDataType.GirlPair, GirlPairId);

        ValidatedSet.SetModIds(ref saveFile.metGirlPairs, MetGirlPairs, GameDataType.GirlPair);
        ValidatedSet.SetModIds(ref saveFile.completedGirlPairs, CompletedGirlPairs, GameDataType.GirlPair);

        SaveUtility.HandleModSaves(GameDataType.Girl, Girls, saveFile.girls, saveFile.girls.Select(x => x.girlId));
        SaveUtility.HandleModSaves(GameDataType.GirlPair, GirlPairs, saveFile.girlPairs, saveFile.girlPairs.Select(x => x.girlPairId));
        SaveUtility.HandleModSaves(GameDataType.Location, FinderSlots, saveFile.finderSlots, saveFile.finderSlots.Select(x => x.locationId));

        var i = 0;
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

    /// <summary>
    /// Picks the game state the save file loads into and makes sure the file is valid for it, falling back to the hub
    /// when it isn't. Runs last in <see cref="SavePipeline"/>, after the file was cleaned, so it only
    /// ever sees ids that exist in the runtime.
    /// </summary>
    public void ResolveGameState(SaveFile saveFile)
    {
        var gameState = DetermineGamestate(saveFile);

        // An unstarted file has no location or pair to validate, it loads into the special state
        if (!saveFile.started) return;

        if (!ModInterface.Data.TryGetDataId(GameDataType.GirlPair, saveFile.girlPairId, out var _))
        {
            ModInterface.Log.Warning($"Pair with runtime {saveFile.girlPairId} not present, defaulting to no pair");
            saveFile.girlPairId = -1;
        }

        if (!ModInterface.Data.TryGetDataId(GameDataType.Location, saveFile.locationId, out var _))
        {
            // the game state has to go with the location, a sim or an event can't be at the hub
            gameState = DefaultToHub(saveFile, $"Location with runtime {saveFile.locationId} not present");
        }

        if (!gameState.ValidateSave(saveFile, out var error))
        {
            gameState = DefaultToHub(saveFile, error);

            if (!gameState.ValidateSave(saveFile, out var hubError))
            {
                throw new Exception($"Hub state invalid after being defaulted to! {hubError}");
            }
        }
    }

    public ModSaveGirl GetGirl(RelativeId id) => Girls.GetOrNew(id);
}