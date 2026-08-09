using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

public sealed class SmoothieStoreHandler : ItemStoreHandler
{
    public const int Priority = 0;
    public const int TargetCount = 4;

    public override IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile)
    {
        var category = new Category<ExpandedItemDefinition>
        {
            TargetCount = TargetCount,
            Priority = Priority,
            Pool = new()
        };

        foreach (var smoothie in Game.Data.Items.GetAll().Select(x => x.GetExpansion()).Where(x => x.StoreHandler == this))
        {
            if (ModInterface.GameData.ExpDisplays.TryGetFirst(x => x.SmoothieItemDef == smoothie.Core, out var expDisplay))
            {
                category.Pool.Add(new(
                    smoothie,
                    Mathf.FloorToInt((1f - expDisplay.Percentage) * 24)));
            }
            else
            {
                var exp = playerFile.GetAffectionLevelExp(smoothie.Affection.Id, false);
                var target = Mathf.Clamp(
                    6 + playerFile.GetBaggageCountByAffectionType(smoothie.Affection.Id, true) * 2,
                    0,
                    24);

                if (exp < target)
                {
                    category.Pool.Add(new(smoothie, target - exp));
                }
            }
        }

        yield return new(ItemTypes.Smoothie, category);
    }

    public override void OnChosen(
        ExpandedItemDefinition item,
        Category<ExpandedItemDefinition> category,
        Category<ExpandedItemDefinition>.Entry entry)
    {
        entry.Weight /= 2;

        if (entry.Weight > 0)
        {
            category.Pool.Add(entry);
        }
    }

    public override bool ShowWithStoreFilter(ItemType highlightedItemType) 
        => highlightedItemType == ItemType.SMOOTHIE;
}