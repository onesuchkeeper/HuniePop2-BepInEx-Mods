// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// overrides store product population to allow for modded smoothies
/// TODO: provide hook for dependant mods to alter populating items and pools
/// </summary>
[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateStoreProducts))]
public static class PlayerFile_PopulateStoreProducts
{
    private static int _storeItemCount => _storeItemPerTypeCount * 4;
    private static int _storeItemPerTypeCount => 8;

    public static bool Prefix(PlayerFile __instance)
    {
        var storeProductFiles = new List<PlayerFileStoreProduct>();
        for (int i = 0; i < _storeItemCount; i++)
        {
            storeProductFiles.Add(__instance.GetPlayerFileStoreProduct(i));
        }

        var selectedSmoothies = new List<(ItemDefinition def, int cost, PuzzleAffectionType currencyType)>();
        var smoothiePool = new List<(ItemDefinition value, int weight)>();
        var modSmoothiePool = new List<(IExpInfo value, int weight)>();

        foreach (var smoothie in Game.Data.Items.GetAllOfTypes(ItemType.SMOOTHIE))
        {
            if (ModInterface.ExpDisplays.TryGetFirst(x => smoothie == x.SmoothieItemDef, out var expDisplay))
            {
                modSmoothiePool.Add((expDisplay, Mathf.FloorToInt((1f - expDisplay.Percentage) * 24)));
            }
            else
            {
                var affectionLevelExp = __instance.GetAffectionLevelExp(smoothie.affectionType, false);
                var num = Mathf.Clamp(6 + __instance.GetBaggageCountByAffectionType(smoothie.affectionType, true) * 2, 0, 24);

                if (affectionLevelExp < num)
                {
                    smoothiePool.Add((smoothie, num - affectionLevelExp));
                }
            }
        }

        while (selectedSmoothies.Count < ModInterface.State.MaxStoreSmoothies
            && (smoothiePool.Count > 0 || modSmoothiePool.Count > 0))
        {
            var modTotalWeight = modSmoothiePool.Sum(x => x.weight);
            var baseTotalWeight = smoothiePool.Sum(x => x.weight);

            if (WeightedBool(baseTotalWeight, modTotalWeight))
            {
                var randomSmoothie = PopWeighted(modSmoothiePool, modTotalWeight);

                if (!selectedSmoothies.Any(x => x.def == randomSmoothie.value.SmoothieItemDef))
                {
                    modSmoothiePool.Add((randomSmoothie.value, randomSmoothie.weight / 2));
                }

                selectedSmoothies.Add((randomSmoothie.value.SmoothieItemDef, 5, (PuzzleAffectionType)Random.Range(0, 4)));
            }
            else
            {
                var randomSmoothie = PopWeighted(smoothiePool, baseTotalWeight);

                if (!selectedSmoothies.Any(x => x.def == randomSmoothie.value))
                {
                    smoothiePool.Add((randomSmoothie.value, randomSmoothie.weight / 2));
                }

                selectedSmoothies.Add((randomSmoothie.value, 5, randomSmoothie.value.affectionType));
            }
        }

        var selectedDateGifts = new List<(ItemDefinition def, int cost)>();
        var dateGifts = Game.Data.Items.GetAllOfTypes([ItemType.DATE_GIFT]);
        while (selectedDateGifts.Count < 12 && dateGifts.Count > 0)
        {
            var itemDefinition = dateGifts.PopRandom();

            if (!__instance.IsItemInInventory(itemDefinition, true, 2)
                && (!itemDefinition.difficultyExclusive
                    || itemDefinition.difficulty == __instance.settingDifficulty))
            {
                selectedDateGifts.Add((itemDefinition, 6));
            }
        }

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false);

        var girlsNeedShoesPool = new List<(GirlDefinition def, int weight)>();
        foreach (var girl in normalGirls)
        {
            var playerFileGirl = __instance.GetPlayerFileGirl(girl);

            int obtainedShoesCount = Mathf.Clamp(playerFileGirl.receivedShoes.Count + __instance.GetInventoryItemsCount(girl.shoesItemDefs, false), 0, girl.shoesItemDefs.Count);

            int remainingShoes = girl.shoesItemDefs.Count - obtainedShoesCount;
            if (remainingShoes > 0 && obtainedShoesCount < playerFileGirl.learnedBaggage.Count + 1)
            {
                girlsNeedShoesPool.Add((girl, remainingShoes * remainingShoes));
            }
        }

        var selectedShoes = new List<(ItemDefinition def, int cost)>();
        while (selectedShoes.Count < 4 && girlsNeedShoesPool.Count > 0)
        {
            var girlShoes = PopWeighted(girlsNeedShoesPool);
            var playerFileGirl = __instance.GetPlayerFileGirl(girlShoes.value);

            var neededShoes = girlShoes.value.shoesItemDefs
                .Where(x => !(__instance.IsItemInInventory(x, false, 1) || playerFileGirl.HasShoes(x)))
                .ToList();

            if (neededShoes.Count > 0)
            {
                var itemDefinition = neededShoes.GetRandom();
                selectedShoes.Add((itemDefinition, 4));
            }
        }

        var girlsNeedUniquePool = new List<(GirlDefinition def, int weight)>();
        foreach (var girl in normalGirls)
        {
            var playerFileGirl = __instance.GetPlayerFileGirl(girl);

            int obtainedUniqueCount = Mathf.Clamp(playerFileGirl.receivedUniques.Count + __instance.GetInventoryItemsCount(girl.uniqueItemDefs, false), 0, girl.uniqueItemDefs.Count);
            int remainingUniques = girl.uniqueItemDefs.Count - obtainedUniqueCount;
            if (remainingUniques > 0 && obtainedUniqueCount < playerFileGirl.learnedBaggage.Count + 1)
            {
                girlsNeedUniquePool.Add((girl, remainingUniques * remainingUniques));
            }
        }

        var selectedUniques = new List<(ItemDefinition def, int cost)>();
        while (selectedUniques.Count < 4 && girlsNeedUniquePool.Count > 0)
        {
            var girlUnique = PopWeighted(girlsNeedUniquePool);
            var playerFileGirl = __instance.GetPlayerFileGirl(girlUnique.value);

            var neededUniques = girlUnique.value.uniqueItemDefs
                .Where(x => !(__instance.IsItemInInventory(x, false, 1) || playerFileGirl.HasUnique(x)))
                .ToList();

            if (neededUniques.Count > 0)
            {
                var itemDefinition = neededUniques[Random.Range(0, neededUniques.Count)];
                selectedUniques.Add((itemDefinition, 4));
            }
        }

        var affectionTypeIndexes = new List<List<int>>
        {
            ListUtils.GetIndexList(_storeItemPerTypeCount, 0),
            ListUtils.GetIndexList(_storeItemPerTypeCount, _storeItemPerTypeCount),
            ListUtils.GetIndexList(_storeItemPerTypeCount, _storeItemPerTypeCount * 2),
            ListUtils.GetIndexList(_storeItemPerTypeCount, _storeItemPerTypeCount * 3)
        };

        foreach (var entry in selectedSmoothies)
        {
            var indexes = affectionTypeIndexes[(int)entry.currencyType];

            if (indexes.Count > 0)
            {
                int selectedSlotIndex = indexes.PopRandom();

                storeProductFiles[selectedSlotIndex].Populate(entry.def, entry.cost);
            }
        }

        foreach (var entry in selectedUniques.Concat(selectedShoes).Concat(selectedDateGifts))
        {
            List<int> slotIndexPool = null;

            switch (entry.def.itemType)
            {
                case ItemType.FOOD:
                case ItemType.DATE_GIFT:
                    if (entry.def.storeSectionPreference && affectionTypeIndexes[(int)entry.def.affectionType].Count > 0)
                    {
                        slotIndexPool = affectionTypeIndexes[(int)entry.def.affectionType];
                    }
                    break;
                case ItemType.SHOES:
                case ItemType.UNIQUE_GIFT:
                    if (affectionTypeIndexes[(int)entry.def.girlDefinition.favoriteAffectionType].Count > 0)
                    {
                        slotIndexPool = affectionTypeIndexes[(int)entry.def.girlDefinition.favoriteAffectionType];
                    }
                    break;
            }

            if (slotIndexPool == null)
            {
                var validIndexes = affectionTypeIndexes.Where(x => x.Count > 0).ToList();
                if (validIndexes.Count == 0)
                {
                    return false;
                }

                slotIndexPool = validIndexes.PopRandom();
            }

            var chosenSlotIndex = slotIndexPool.PopRandom();

            foreach (var indexes in affectionTypeIndexes)
            {
                indexes.Remove(chosenSlotIndex);
            }

            storeProductFiles[chosenSlotIndex].Populate(entry.def, entry.cost);
        }

        var foodItems = Game.Data.Items.GetAllOfTypes([ItemType.FOOD]);
        ListUtils.ShuffleList(foodItems);

        //staminaFoodLimit is always assigned here, and this is the only place where its value is read functionally,
        //so it's actually meaningless and its value is always 4...
        __instance.staminaFoodLimit = 4;
        __instance.staminaFoodLimit = Mathf.Clamp(__instance.staminaFoodLimit, 0, 4);
        var foodPool = foodItems.Limit(__instance.staminaFoodLimit, x => x.noStaminaCost).ToList();

        foreach (var index in affectionTypeIndexes.SelectMany(x => x))
        {
            var food = foodPool.PopFirst();

            storeProductFiles[index].Populate(food, food.storeCost);

            if (foodPool.Count == 0)
            {
                break;
            }
        }

        return false;
    }

    private static (T value, int weight) PopWeighted<T>(List<(T value, int weight)> values, int totalWeight)
    {
        var selectedWeight = Random.Range(0, totalWeight);
        var currentWeight = 0;

        foreach (var weightedValue in values)
        {
            currentWeight += weightedValue.weight;

            if (currentWeight >= selectedWeight)
            {
                values.Remove(weightedValue);
                return weightedValue;
            }
        }

        throw new System.Exception("Failed to pop weighted");
    }

    private static (T value, int weight) PopWeighted<T>(List<(T value, int weight)> values)
        => PopWeighted(values, values.Sum(x => x.weight));

    private static bool WeightedBool(int weightFalse, int weightTrue)
        => weightTrue > Random.Range(0, weightFalse + weightTrue);
}
