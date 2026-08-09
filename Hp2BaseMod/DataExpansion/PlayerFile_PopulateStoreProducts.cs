// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateStoreProducts))]
public static class PlayerFile_PopulateStoreProducts
{
    private const int STORE_ITEMS_PER_TYPE = 8;
    private const int STORE_ITEMS_TYPE_COUNT = 4;
    private const int STORE_ITEM_COUNT = STORE_ITEMS_PER_TYPE * STORE_ITEMS_TYPE_COUNT;

    public static bool Prefix(PlayerFile __instance)
    {
        var args = new StoreProductsPopulateArgs();

        foreach (var handler in ModInterface.GameData.ItemStoreHandlers.Values)
        {
            foreach (var category in handler.CreateStoreCategories(__instance))
            {
                args.ItemCategories.Add(category.Key, category.Value);
            }
        }
        
        ModInterface.Events.NotifyPopulateStoreProducts(args);
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
                ListUtils.ShuffleList(entry.Value.Pool);
            }

            while (weightedCategories.Any())
            {
                if (affectionTypeIndexes.All(x => x.Value.Count == 0)) return false;

                var categoryEntry = Category<Category<ExpandedItemDefinition>>.GetWeighted(weightedCategories);
                var category = categoryEntry.Value;

                var selection = Category<ExpandedItemDefinition>.PopWeighted(category.Pool);

                categoryEntry.Weight -= selection.Weight;
                if (categoryEntry.Weight <= 0)
                {
                    weightedCategories.Remove(categoryEntry);
                }

                if (selection.Value == null)
                {
                    continue;
                }

                selection.Value.StoreHandler.OnChosen(
                    selection.Value,
                    category,
                    selection);

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

                if (category.TargetCount <= 0)
                {
                    weightedCategories.Remove(categoryEntry);
                }

            }
        }

        return false;
    }

    private static bool TrySelectSlot(
        ExpandedItemDefinition item,
        Dictionary<RelativeId, List<int>> affectionTypeIndexes,
        List<PlayerFileStoreProduct> storeSlots,
        out PlayerFileStoreProduct slot)
    {
        List<int> slotPool = null;

        var handler = item.StoreHandler;

        var pref = handler.GetPreferredStoreSection(item);

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
