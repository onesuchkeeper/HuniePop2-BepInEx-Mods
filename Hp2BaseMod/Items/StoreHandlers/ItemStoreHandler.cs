namespace Hp2BaseMod;

using System.Collections.Generic;
using Hp2BaseMod.Elements;

public abstract class ItemStoreHandler : IItemStoreHandler
{
    public abstract IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile);

    public virtual void OnChosen(ExpandedItemDefinition item, Category<ExpandedItemDefinition> category, Category<ExpandedItemDefinition>.Entry entry)
    {
        //noop
    }

    public virtual RelativeId? GetPreferredStoreSection(ExpandedItemDefinition item) => item.Core.storeSectionPreference 
        ? item.Affection.Id 
        : null;

    public abstract bool ShowWithStoreFilter(ItemType highlightedItemType);
}