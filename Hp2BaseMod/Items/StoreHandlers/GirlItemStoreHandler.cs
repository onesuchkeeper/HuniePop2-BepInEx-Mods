using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

public abstract class GirlItemStoreHandler : ItemStoreHandler
{
    protected const int Priority = 1_000_000;
    protected const int TargetCount = 4;

    protected static void AddItems(
        List<ExpandedItemDefinition> items,
        HashSet<int> owned,
        int maxItems,
        List<Category<ExpandedItemDefinition>.Entry> pool)
    {
        var remaining = items
            .Where(x => !owned.Contains(x.Core.id))
            .ToList();

        var ownedCount = items.Count - remaining.Count;

        if (ownedCount < maxItems &&
            remaining.Count > 0)
        {
            pool.Add(new(
                remaining.GetRandom(),
                remaining.Count * remaining.Count));
        }
    }

    public override RelativeId? GetPreferredStoreSection(ExpandedItemDefinition item)
    {
        if (!item.Core.storeSectionPreference) return null;

        if (item.Core.girlDefinition == null)
        {
            ModInterface.Log.Error($"{item.Core.itemName} {item.Core.ModId()} doesn't have a girlDefinition!");
            return null;
        }

        return item.Core.girlDefinition.GetExpansion().FavAffection.Id;
    }
}
