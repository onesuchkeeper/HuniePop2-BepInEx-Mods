using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public sealed class UniqueItemStoreHandler : GirlItemStoreHandler
{
    public override IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile)
    {
        var category = new Category<ExpandedItemDefinition>
        {
            TargetCount = TargetCount,
            Priority = Priority,
            Pool = new()
        };

        var owned = new HashSet<int>();

        foreach (var id in playerFile.inventorySlots.Where(x => x.itemDefinition != null && x.itemDefinition.GetExpansion().StoreHandler == this).Select(x => x.itemDefinition.id))
        {
            owned.Add(id);
        }

        foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
        {
            var pfg = playerFile.GetPlayerFileGirl(girl);

            foreach (var item in girl.uniqueItemDefs.Where(pfg.HasUnique))
            {
                owned.Add(item.id);
            }
        }

        foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
        {
            var pfg = playerFile.GetPlayerFileGirl(girl);
            var maxItems = pfg.learnedBaggage.Count + 1;

            AddItems(
                girl.uniqueItemDefs.Select(x => x.GetExpansion()).ToList(),
                owned,
                maxItems,
                category.Pool);
        }

        yield return new(ItemTypes.Unique, category);
    }

    public override bool ShowWithStoreFilter(ItemType highlightedItemType) 
        => highlightedItemType == ItemType.UNIQUE_GIFT;
}
