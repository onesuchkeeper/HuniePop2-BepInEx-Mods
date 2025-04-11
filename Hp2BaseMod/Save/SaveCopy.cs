using System.Linq;

namespace Hp2BaseMod.Save;

public static class SaveCopy_Ext
{
    public static SaveData Copy(this SaveData saveData) => saveData == null
        ? null
        : new SaveData(0)
        {
            windowMode = saveData.windowMode,
            resolutionIndex = saveData.resolutionIndex,
            censoredMode = saveData.censoredMode,
            musicVol = saveData.musicVol,
            soundVol = saveData.soundVol,
            voiceVol = saveData.voiceVol,
            censorPatched = saveData.censorPatched,
            unlockedCodes = saveData.unlockedCodes.ToList(),
            files = saveData.files?.Select(x => x.Copy()).ToList()
        };

    public static SaveFile Copy(this SaveFile file) => file == null
        ? null
        : new SaveFile()
        {
            started = file.started,
            fileIconGirlId = file.fileIconGirlId,
            settingGender = file.settingGender,
            settingDifficulty = file.settingDifficulty,
            storyProgress = file.storyProgress,
            daytimeElapsed = file.daytimeElapsed,
            finderRestockTime = file.finderRestockTime,
            storeRestockDay = file.storeRestockDay,
            staminaFoodLimit = file.staminaFoodLimit,
            fruitCounts = file.fruitCounts.ToArray(),
            affectionLevelExps = file.affectionLevelExps.ToArray(),
            relationshipPoints = file.relationshipPoints,
            alphaDateCount = file.alphaDateCount,
            nonstopDateCount = file.nonstopDateCount,
            locationId = file.locationId,
            girlPairId = file.girlPairId,
            sidesFlipped = file.sidesFlipped,
            girls = file.girls?.Select(x => x.Copy()).ToList(),
            girlPairs = file.girlPairs?.Select(x => x.Copy()).ToList(),
            metGirlPairs = file.metGirlPairs.ToList(),
            completedGirlPairs = file.completedGirlPairs.ToList(),
            finderSlots = file.finderSlots?.Select(x => x.Copy()).ToList(),
            inventorySlots = file.inventorySlots?.Select(x => x.Copy()).ToList(),
            storeProducts = file.storeProducts?.Select(x => x.Copy()).ToList(),
            flags = file.flags?.Select(x => x.Copy()).ToList()
        };

    public static SaveFileFlag Copy(this SaveFileFlag flag) => flag == null
        ? null
        : new SaveFileFlag(flag.flagName)
        {
            //flagName set in constructor
            flagValue = flag.flagValue
        };

    public static SaveFileStoreProduct Copy(this SaveFileStoreProduct product) => product == null
        ? null
        : new SaveFileStoreProduct(product.productIndex)
        {
            //productIndex set in constructor
            itemId = product.itemId,
            itemCost = product.itemCost
        };

    public static SaveFileInventorySlot Copy(this SaveFileInventorySlot slot) => slot == null
        ? null
        : new SaveFileInventorySlot(slot.slotIndex)
        {
            //slotIndex set in constructor
            itemId = slot.itemId,
            daytimeStamp = slot.daytimeStamp
        };

    public static SaveFileFinderSlot Copy(this SaveFileFinderSlot slot) => slot == null
        ? null
        : new SaveFileFinderSlot(slot.locationId)
        {
            //locationId set in constructor
            girlPairId = slot.girlPairId,
            sidesFlipped = slot.sidesFlipped
        };

    public static SaveFileGirlPair Copy(this SaveFileGirlPair pair) => pair == null
        ? null
        : new SaveFileGirlPair(pair.girlPairId)
        {
            //girlPairId set in constructor
            relationshipLevel = pair.relationshipLevel,
            learnedFavs = pair.learnedFavs.ToList(),
            recentFavQuestions = pair.recentFavQuestions.ToList()
        };

    public static SaveFileGirl Copy(this SaveFileGirl girl) => girl == null
        ? null
        : new SaveFileGirl(girl.girlId)
        {
            //girlId set in constructor
            playerMet = girl.playerMet,
            relationshipPoints = girl.relationshipPoints,
            relationshipUpCount = girl.relationshipUpCount,
            activeBaggageIndex = girl.activeBaggageIndex,
            hairstyleIndex = girl.hairstyleIndex,
            outfitIndex = girl.outfitIndex,
            staminaFreeze = girl.staminaFreeze,
            stylesOnDates = girl.stylesOnDates,
            learnedBaggage = girl.learnedBaggage.ToList(),
            receivedUniques = girl.receivedUniques.ToList(),
            receivedShoes = girl.receivedShoes.ToList(),
            learnedFavs = girl.learnedFavs.ToList(),
            unlockedHairstyles = girl.unlockedHairstyles.ToList(),
            unlockedOutfits = girl.unlockedOutfits.ToList(),
            recentHerQuestions = girl.recentHerQuestions.ToList(),
            dateGiftSlots = girl.dateGiftSlots?.Select(x => x.Copy()).ToList()
        };
}