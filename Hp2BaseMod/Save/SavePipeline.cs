// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Linq;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod.Save;

/// <summary>
/// Injects, cleans and validates one <see cref="SaveFile"/>. The stages are the same for every source of a
/// save file, a source only differs in what it does to the mod data first, see <see cref="Prepare"/>.
/// </summary>
internal abstract class SavePipeline
{
    public int FileIndex { get; }

    protected SavePipeline(int fileIndex)
    {
        FileIndex = fileIndex;
    }

    /// <summary>
    /// 1. prepare the mod data for the source
    /// 2. inject the mod data into the file
    /// 3. clean out everything that points at data that isn't in the runtime
    /// 4. resolve the game state and validate the file for it, falling back to the hub
    /// </summary>
    public void Process(SaveFile saveFile)
    {
        using (ModInterface.Log.MakeIndent($"Processing save file {FileIndex} ({GetType().Name})"))
        {
            Prepare();

            var modFile = ModInterface.Save.GetFile(FileIndex);

            modFile.SetData(saveFile);
            Clean(saveFile, modFile);
            modFile.ResolveGameState(saveFile);
        }
    }

    /// <summary>
    /// Runs before the mod data is injected, the place for a source to change what the file starts with
    /// </summary>
    protected virtual void Prepare() { }

    private void Clean(SaveFile saveFile, ModSaveFile modSaveFile)
    {
        saveFile.flags ??= new();
        var wardrobeFlag = saveFile.flags.FirstOrDefault(x => x.flagName == Flags.WARDROBE_GIRL_ID);
        if (wardrobeFlag != null && !ModInterface.Data.TryGetDataId(GameDataType.Girl, wardrobeFlag.flagValue, out _))
        {
            wardrobeFlag.flagValue = Girls.Ashley.LocalId;
        }

        if (!ModInterface.Data.TryGetDataId(GameDataType.Girl, saveFile.fileIconGirlId, out _))
        {
            saveFile.fileIconGirlId = Girls.Kyu.LocalId;
        }

        // location and pair are validated together with the game state in ModSaveFile.ResolveGameState

        saveFile.girls = saveFile.girls?.Where(x => TryCleanGirl(x, modSaveFile)).ToList() ?? new();
        saveFile.girlPairs = saveFile.girlPairs?.Where(TryCleanPair).ToList() ?? new();
        saveFile.finderSlots = saveFile.finderSlots?.Where(TryCleanFinderSlot).ToList() ?? new();
        saveFile.inventorySlots = saveFile.inventorySlots?.Where(TryCleanInvSlot).ToList() ?? new();
        saveFile.storeProducts = saveFile.storeProducts?.Where(TryCleanStoreProduct).ToList() ?? new();
    }

    private bool TryCleanGirl(SaveFileGirl saveGirl, ModSaveFile modSaveFile)
    {
        if (saveGirl == null) return false;
        if (!ModInterface.Data.TryGetDataId(GameDataType.Girl, saveGirl.girlId, out var girlId)) return false;

        var def = Game.Data.Girls.Get(saveGirl.girlId);
        var expansion = def.GetExpansion();
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

        // Convert legacy 0-based question indices to runtime IDs if needed before filtering
        var allQuestions = Game.Data.Questions.GetAll();
        saveGirl.learnedFavs = saveGirl.learnedFavs
            .Select(fav => (fav >= 0 && fav < allQuestions.Count && !ModInterface.Data.TryGetDataId(GameDataType.Question, fav, out _))
                ? allQuestions[fav].id
                : fav)
            .Where(fav => ModInterface.Data.TryGetDataId(GameDataType.Question, fav, out _))
            .Distinct()
            .ToList();

        saveGirl.recentHerQuestions = saveGirl.recentHerQuestions
            .Where(x => IsIndexInCollection(x, def.herQuestions.Count))
            .Distinct()
            .ToList();

        saveGirl.dateGiftSlots = saveGirl.dateGiftSlots.Where(TryCleanInvSlot).ToList();

        return true;
    }

    private bool TryCleanPair(SaveFileGirlPair pair)
    {
        if (pair == null) return false;

        pair.learnedFavs = pair.learnedFavs
            .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Question, x, out _))
            .Distinct()
            .ToList();

        pair.recentFavQuestions = pair.recentFavQuestions
            .Where(x => ModInterface.Data.TryGetDataId(GameDataType.Question, x, out _))
            .Distinct()
            .ToList();

        return true;
    }

    private bool TryCleanStoreProduct(SaveFileStoreProduct product)
    {
        if (product == null) return false;

        if (!ModInterface.Data.TryGetDataId(GameDataType.Item, product.itemId, out _))
        {
            product.itemId = -1;
        }

        return true;
    }

    private bool TryCleanInvSlot(SaveFileInventorySlot invSlot)
    {
        if (invSlot == null) return false;

        if (!ModInterface.Data.TryGetDataId(GameDataType.Item, invSlot.itemId, out _))
        {
            invSlot.itemId = -1;
        }

        return true;
    }

    private bool TryCleanFinderSlot(SaveFileFinderSlot finderSlot)
    {
        if (finderSlot == null) return false;
        if (!ModInterface.Data.TryGetDataId(GameDataType.Location, finderSlot.locationId, out _)) return false;

        if (!ModInterface.Data.TryGetDataId(GameDataType.GirlPair, finderSlot.girlPairId, out _))
        {
            finderSlot.girlPairId = -1;
        }

        return true;
    }

    private bool IsIndexInCollection(int index, int count)
        => (count > 0) && (index >= 0) && (index < count);
}