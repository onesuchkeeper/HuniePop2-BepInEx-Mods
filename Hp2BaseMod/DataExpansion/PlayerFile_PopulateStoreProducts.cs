// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateStoreProducts))]
public static class PlayerFile_PopulateStoreProducts
{
    private const int STAMINA_FOOD_ITEM_PRIORITY = 3_000_000;
    private const int GIRL_ITEM_PRIORITY = 1_000_000;
    private const int DATE_ITEM_PRIORITY = 2_000_000;
    private const int FOOD_ITEM_PRIORITY = int.MaxValue;
    private const int SMOOTHIE_ITEM_PRIORITY = 0;

    private const int STORE_ITEM_COUNT = STORE_ITEMS_PER_TYPE * STORE_ITEMS_TYPE_COUNT;
    private const int STORE_ITEMS_PER_TYPE = 8;
    private const int STORE_ITEMS_TYPE_COUNT = 4;

    /// <summary>
    /// The number of smoothies to attempt to populate in the store.
    /// </summary>
    public static int StoreSmoothieTarget = 4;

    /// <summary>
    /// The number of date gifts to attempt to populate in the store.
    /// </summary>
    public static int StoreDateGiftTarget = 12;

    /// <summary>
    /// The number of shoes to attempt to populate in the store.
    /// </summary>
    public static int StoreShoeTarget = 4;

    /// <summary>
    /// The number of uniques to attempt to populate in the store.
    /// </summary>
    public static int StoreUniqueTarget = 4;

    /// <summary>
    /// The number of stamina foods to attempt to populate in the store.
    /// </summary>
    public static int StoreStaminaFoodTarget = 4;

    public static bool Prefix(PlayerFile __instance)
    {
        var args = new StoreProductsPopulateArgs();

        args.ItemCategories[ItemTypes.Smoothie] = CreateSmoothiesCategory(__instance);
        args.ItemCategories[ItemTypes.DateGift] = CreateDateGiftCategory(__instance);
        AddGirlCategories(__instance, args);
        AddFoodCategories(args);

        ModInterface.Events.NotifyPopulateStoreProducts(args);

        var affectionTypeIndexes = Enumerable.Range(0, STORE_ITEMS_TYPE_COUNT)
            .Select(i => ListUtils.GetIndexList(STORE_ITEMS_PER_TYPE, i * STORE_ITEMS_PER_TYPE))
            .ToArray();

        var storeProductFiles = Enumerable.Range(0, STORE_ITEM_COUNT)
            .Select(__instance.GetPlayerFileStoreProduct)
            .ToList();

        foreach (var grouping in args.ItemCategories.Values
            .Where(x => x != null
                && x.Pool != null
                && x.Pool.Any()
                && x.TargetCount > 0)
            .GroupBy(x => x.Priority)
            .OrderBy(x => x.Key))
        {
            var weightedCategories = grouping
                .Select(x => x.Pool.Sum(x => x.Weight))
                .Zip(grouping, (weight, category) => new Category<Category<ItemDefinition>>.Entry(category, weight))
                .Where(x => x.Weight > 0)
                .ToList();

            foreach (var entry in weightedCategories)
            {
                ListUtils.ShuffleList(entry.Value.Pool);
            }

            while (weightedCategories.Any())
            {
                // if no more slots are available return
                if (affectionTypeIndexes.All(x => x.Count == 0))
                {
                    return false;
                }

                var categoryEntry = Category<Category<ItemDefinition>>.GetWeighted(weightedCategories);
                var category = categoryEntry.Value;

                // pick and remove an item from the pool
                var selection = Category<ItemDefinition>.PopWeighted(category.Pool);
                categoryEntry.Weight -= selection.Weight;
                if (categoryEntry.Weight <= 0)
                {
                    weightedCategories.Remove(categoryEntry);
                }

                if (selection.Value == null) continue;

                category.OnEntryChosen?.Invoke(selection);

                if (TrySelectSlot(selection.Value, affectionTypeIndexes, storeProductFiles, out var slot))
                {
                    slot.Populate(selection.Value, selection.Value.storeCost);

                    category.TargetCount--;
                    if (category.TargetCount < 1)
                    {
                        weightedCategories.Remove(categoryEntry);
                    }
                }
            }
        }

        return false;
    }

    private static Category<ItemDefinition> CreateSmoothiesCategory(PlayerFile playerFile)
    {
        var smoothiesCategory = new Category<ItemDefinition>()
        {
            TargetCount = StoreSmoothieTarget,
            Priority = SMOOTHIE_ITEM_PRIORITY,
            Pool = new()
        };

        // this is the base game's logic for smoothie items
        var selectedSmoothies = new HashSet<ItemDefinition>();
        smoothiesCategory.OnEntryChosen = entry =>
        {
            if (!selectedSmoothies.Contains(entry.Value))
            {
                selectedSmoothies.Add(entry.Value);
                entry.Weight /= 2;
                smoothiesCategory.Pool.Add(entry);
            }
        };

        foreach (var smoothie in Game.Data.Items.GetAllOfTypes(ItemType.SMOOTHIE))
        {
            if (ModInterface.ExpDisplays.TryGetFirst(x => smoothie == x.SmoothieItemDef, out var expDisplay))
            {
                smoothiesCategory.Pool.Add(new(expDisplay.SmoothieItemDef, Mathf.FloorToInt((1f - expDisplay.Percentage) * 24)));
            }
            else
            {
                var affectionLevelExp = playerFile.GetAffectionLevelExp(smoothie.affectionType, false);
                var num = Mathf.Clamp(6 + playerFile.GetBaggageCountByAffectionType(smoothie.affectionType, true) * 2, 0, 24);

                if (affectionLevelExp < num)
                {
                    smoothiesCategory.Pool.Add(new(smoothie, num - affectionLevelExp));
                }
            }
        }

        return smoothiesCategory;
    }

    private static Category<ItemDefinition> CreateDateGiftCategory(PlayerFile playerFile)
    {
        var dateGiftCategory = new Category<ItemDefinition>()
        {
            TargetCount = StoreDateGiftTarget,
            Priority = DATE_ITEM_PRIORITY,
            Pool = Game.Data.Items.GetAllOfTypes([ItemType.DATE_GIFT])
                .Where(x => !playerFile.IsItemInInventory(x, true, 2)
                            && (!x.difficultyExclusive
                                || x.difficulty == playerFile.settingDifficulty))
                .Select(x => new Category<ItemDefinition>.Entry(x, 1))
                .ToList()
        };


        return dateGiftCategory;
    }

    private static void AddGirlCategories(PlayerFile playerFile, StoreProductsPopulateArgs args)
    {
        var shoesCategory = new Category<ItemDefinition>()
        {
            TargetCount = StoreShoeTarget,
            Priority = GIRL_ITEM_PRIORITY,
            Pool = new()
        };
        args.ItemCategories[ItemTypes.Shoe] = shoesCategory;

        var uniqueCategory = new Category<ItemDefinition>()
        {
            TargetCount = StoreUniqueTarget,
            Priority = GIRL_ITEM_PRIORITY,
            Pool = new()
        };
        args.ItemCategories[ItemTypes.Unique] = uniqueCategory;

        void handleGirlItems(List<ItemDefinition> itemDefs,
            List<int> receivedItems,
            int maxItemCount,
            List<Category<ItemDefinition>.Entry> pool)
        {
            int ownedItemCount =
                    receivedItems.Count +
                    playerFile.GetInventoryItemsCount(itemDefs, false);

            if (ownedItemCount < maxItemCount)
            {
                var remainingItems = itemDefs
                    .Where(x =>
                        !playerFile.IsItemInInventory(x, false) &&
                        !receivedItems.Contains(x.id))
                    .ToList();

                if (remainingItems.Count > 0)
                {
                    pool.Add(new(remainingItems.GetRandom(), remainingItems.Count * remainingItems.Count));
                }
            }
        }

        foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
        {
            var pfg = playerFile.GetPlayerFileGirl(girl);
            var maxItems = pfg.learnedBaggage.Count + 1;
            handleGirlItems(girl.shoesItemDefs, pfg.receivedShoes, maxItems, shoesCategory.Pool);
            handleGirlItems(girl.uniqueItemDefs, pfg.receivedUniques, maxItems, uniqueCategory.Pool);
        }
    }

    private static void AddFoodCategories(StoreProductsPopulateArgs args)
    {
        var foodItems = Game.Data.Items.GetAllOfTypes([ItemType.FOOD]);

        var foodCategory = new Category<ItemDefinition>()
        {
            TargetCount = int.MaxValue,
            Priority = FOOD_ITEM_PRIORITY,
            Pool = foodItems
                .Where(x => !x.noStaminaCost)
                .Select(x => new Category<ItemDefinition>.Entry(x, 1))
                .ToList()
        };
        args.ItemCategories[ItemTypes.Food] = foodCategory;

        var staminaFoodCategory = new Category<ItemDefinition>()
        {
            TargetCount = StoreStaminaFoodTarget,
            Priority = STAMINA_FOOD_ITEM_PRIORITY,
            Pool = foodItems
                .Where(x => x.noStaminaCost)
                .Select(x => new Category<ItemDefinition>.Entry(x, 1))
                .ToList()
        };
        args.ItemCategories[ItemTypes.StaminaFood] = staminaFoodCategory;
    }

    private static bool TrySelectSlot(ItemDefinition item, List<int>[] affectionTypeIndexes, List<PlayerFileStoreProduct> storeProductFiles, out PlayerFileStoreProduct slot)
    {
        List<int> slotIndexPool = null;

        // determine index pool
        switch (item.itemType)
        {
            case ItemType.SHOES:
            case ItemType.UNIQUE_GIFT:
                if (item.storeSectionPreference)
                {
                    var typeIndex = (int)item.girlDefinition.favoriteAffectionType;

                    if (typeIndex >= 0
                        && typeIndex < affectionTypeIndexes.Length)
                    {
                        slotIndexPool = affectionTypeIndexes[typeIndex];
                    }
                    else
                    {
                        ModInterface.Log.Error($"Invalid affection type index on item {item.name}");
                        slot = null;
                        return false;
                    }
                }
                break;
            default:
                if (item.storeSectionPreference)
                {
                    var typeIndex = (int)item.affectionType;

                    if (typeIndex >= 0
                        && typeIndex < affectionTypeIndexes.Length)
                    {
                        slotIndexPool = affectionTypeIndexes[typeIndex];
                    }
                    else
                    {
                        ModInterface.Log.Error($"Invalid affection type index on item {item.name}");
                        slot = null;
                        return false;
                    }
                }
                break;
        }

        if (slotIndexPool == null)
        {
            // already confirmed that there will be at least one
            slotIndexPool = affectionTypeIndexes.Where(x => x.Count > 0).ToArray().GetRandom();
        }
        else if (slotIndexPool.Count == 0)
        {
            slot = null;
            return false;
        }

        slot = storeProductFiles[slotIndexPool.PopRandom()];
        return true;
    }
}
