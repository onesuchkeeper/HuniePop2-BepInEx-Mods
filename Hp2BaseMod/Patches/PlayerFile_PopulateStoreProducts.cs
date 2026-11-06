// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// overrides the base game's store population to provide hooks
/// to allow mods to add additional items
/// </summary>
[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateStoreProducts))]
public static class PlayerFile_PopulateStoreProducts
{
    private const int STORE_ITEMS_PER_TYPE = 8;
    private const int STORE_ITEMS_TYPE_COUNT = 4;
    private const int STORE_ITEM_COUNT = STORE_ITEMS_PER_TYPE * STORE_ITEMS_TYPE_COUNT;

    public static bool Prefix(PlayerFile __instance)
    {

        using (ModInterface.Log.MakeIndent("PopulateStoreProducts"))
        {
            __instance.inventorySlots ??= new();
            __instance.storeProducts ??= new();
            __instance.girls ??= new();
            __instance.girlPairs ??= new();
            __instance.flags ??= new();
            __instance.metGirlPairs ??= new();
            __instance.completedGirlPairs ??= new();
            __instance.finderSlots ??= new();

            var args = new StoreProductsPopulateArgs();

            foreach (var handlerEntry in ModInterface.GameData.ItemStoreHandlers)
            {
                var handler = handlerEntry.Value;
                if (handler == null)
                {
                    ModInterface.Log.Error($"PopulateStoreProducts: Item store handler {handlerEntry.Key} is NULL, skipping");
                    continue;
                }

                var categories = handler.CreateStoreCategories(__instance);

                if (categories == null) continue;

                foreach (var category in categories)
                {
                    if (category.Value == null)
                    {
                        ModInterface.Log.Error($"PopulateStoreProducts: Handler {handlerEntry.Key} returned NULL category for key {category.Key}");
                        continue;
                    }

                    if (category.Value.Pool == null)
                    {
                        ModInterface.Log.Error($"PopulateStoreProducts: Handler {handlerEntry.Key}, category {category.Key} has NULL pool");
                        continue;
                    }

                    ModInterface.Log.Message($"PopulateStoreProducts: Handler {handlerEntry.Key}, category {category.Key}, " 
                        + $"pool={category.Value.Pool.Count}, target={category.Value.TargetCount}");

                    if (args.ItemCategories.ContainsKey(category.Key))
                    {
                        ModInterface.Log.Error($"PopulateStoreProducts: Handler {handlerEntry.Key} returned duplicate category key {category.Key}, ignoring");
                        continue;
                    }

                    args.ItemCategories.Add(category.Key, category.Value);
                }
            }

            ModInterface.Events.NotifyPopulateStoreProducts(args);

            // Mods can modify the categories (or add their own) in the event above,
            // so nothing in args can be trusted past this point.
            SanitizeCategories(args);
            var affectionTypeIndexes = new Dictionary<RelativeId, List<int>>();
            var next = 0;

            foreach (var key in new[] { 
                PuzzleAffectionId.Talent, 
                PuzzleAffectionId.Flirtation, 
                PuzzleAffectionId.Romance, 
                PuzzleAffectionId.Sexuality })
            {
                affectionTypeIndexes[key] = Enumerable.Range(next, STORE_ITEMS_PER_TYPE).ToList();
                next += STORE_ITEMS_PER_TYPE;
            }

            var storeSlots = Enumerable.Range(0, STORE_ITEM_COUNT)
                .Select(__instance.GetPlayerFileStoreProduct)
                .ToList();
            
            foreach (var grouping in args.ItemCategories.Values
                .Where(x =>
                    x != null &&
                    x.TargetCount > 0 &&
                    x.Pool != null &&
                    x.Pool.Any())
                .GroupBy(x => x.Priority)
                .OrderBy(x => x.Key))
            {
                var weightedCategories = grouping
                    .Select(c => new Category<Category<ExpandedItemDefinition>>.Entry(
                        c,
                        c.Pool.Sum(e => e.Weight)))
                    .Where(e => e.Weight > 0)
                    .ToList();

                foreach (var entry in weightedCategories)
                {
                    entry.Value.Pool.Shuffle();
                }

                while (weightedCategories.Any())
                {
                    if (affectionTypeIndexes.All(x => x.Value.Count == 0)) return false;

                    var categoryEntry = Category<Category<ExpandedItemDefinition>>.GetWeighted(weightedCategories);
                    var category = categoryEntry.Value;

                    if (category.Pool == null || category.Pool.Count == 0)
                    {
                        weightedCategories.Remove(categoryEntry);
                        continue;
                    }

                    var selection = Category<ExpandedItemDefinition>.PopWeighted(category.Pool);

                    if (selection.Value != null)
                    {
                        // May modify the entry and/or put it back in the pool (e.g. smoothies re-add at half weight)
                        selection.Value.StoreHandler?.OnChosen(
                            selection.Value,
                            category,
                            selection);
                    }

                    // The pool's weight can change in OnChosen, so re-sync the tracked weight
                    // from the pool itself rather than subtracting the popped weight.
                    categoryEntry.Weight = category.Pool.Sum(e => e.Weight);
                    if (categoryEntry.Weight <= 0)
                    {
                        weightedCategories.Remove(categoryEntry);
                    }

                    if (selection.Value == null)
                    {
                        continue;
                    }

                    if (!TrySelectSlot(
                            selection.Value,
                            affectionTypeIndexes,
                            storeSlots,
                            out var slot))
                    {
                        continue;
                    }

                    slot.Populate(selection.Value.Core, selection.Value.Core.storeCost);

                    category.TargetCount--;

                    if (category.TargetCount <= 0 || category.Pool.Count == 0)
                    {
                        weightedCategories.Remove(categoryEntry);
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Removes anything from the categories that would cause the population loop to throw.
    /// Everything removed is logged with enough info to find the offending mod/item.
    /// </summary>
    private static void SanitizeCategories(StoreProductsPopulateArgs args)
    {
        var invalidKeys = new List<RelativeId>();

        foreach (var categoryEntry in args.ItemCategories)
        {
            var key = categoryEntry.Key;
            var category = categoryEntry.Value;

            if (category == null || category.Pool == null)
            {
                ModInterface.Log.Error($"PopulateStoreProducts: Category {key} is null or has a null pool after event, removing");
                invalidKeys.Add(key);
                continue;
            }

            category.Pool.RemoveAll(entry =>
            {
                if (entry == null)
                {
                    ModInterface.Log.Error($"PopulateStoreProducts: Category {key} contains a NULL entry, removing");
                    return true;
                }

                if (entry.Weight < 0)
                {
                    ModInterface.Log.Error($"PopulateStoreProducts: Category {key} contains an entry with negative weight {entry.Weight}, removing");
                    return true;
                }

                if (entry.Value == null || entry.Value.Core == null)
                {
                    ModInterface.Log.Error($"PopulateStoreProducts: Category {key} contains an entry with a null item, removing");
                    return true;
                }

                if (entry.Value.StoreHandler == null)
                {
                    ModInterface.Log.Error($"PopulateStoreProducts: Category {key}, item {entry.Value.Core.itemName} "
                        + $"{entry.Value.Core.ModId()} has no StoreHandler (is its ItemStoreHandlerId unset or unregistered?), removing");
                    return true;
                }

                return false;
            });
        }

        foreach (var key in invalidKeys)
        {
            args.ItemCategories.Remove(key);
        }
    }

    private static bool TrySelectSlot(
        ExpandedItemDefinition item,
        Dictionary<RelativeId, List<int>> affectionTypeIndexes,
        List<PlayerFileStoreProduct> storeSlots,
        out PlayerFileStoreProduct slot)
    {
        List<int> slotPool = null;

        var handler = item.StoreHandler;

        var pref = handler?.GetPreferredStoreSection(item);

        if (pref.HasValue)
        {
            if (!affectionTypeIndexes.TryGetValue(pref.Value, out slotPool))
            {
                ModInterface.Log.Error($"Invalid preferred store section on item {item.Core.name}");
                slot = null;
                return false;
            }
        }

        if (slotPool == null)
        {
            slotPool = affectionTypeIndexes.Values
                .Where(x => x.Count > 0)
                .ToArray()
                .GetRandom();
        }
        else if (slotPool.Count == 0)
        {
            slot = null;
            return false;
        }

        slot = storeSlots[slotPool.PopRandom()];
        return true;
    }
}