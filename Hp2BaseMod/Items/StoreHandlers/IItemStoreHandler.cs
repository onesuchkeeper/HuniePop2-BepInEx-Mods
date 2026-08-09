
using System.Collections.Generic;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public interface IItemStoreHandler
{
    /// <summary>
    /// Creates zero or more store categories owned by this handler.
    /// The returned categories should already contain their populated pools,
    /// target counts, priorities, weights, and callbacks.
    /// </summary>
    IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(
        PlayerFile playerFile);

    /// <summary>
    /// Invoked after an item has been selected from one of this handler's
    /// categories. Implementations may modify the selected entry or reinsert
    /// it into the category (for example, smoothie behavior).
    /// </summary>
    void OnChosen(
        ExpandedItemDefinition item,
        Category<ExpandedItemDefinition> category,
        Category<ExpandedItemDefinition>.Entry entry);

    /// <summary>
    /// Returns the preferred store section for this item.
    /// Only called when HasPreferredStoreSection returns true.
    /// </summary>
    RelativeId? GetPreferredStoreSection(ExpandedItemDefinition item);

    /// <summary>
    /// If the item should be show when the given filter is active
    /// </summary>
    /// <param name="highlightedItemType"></param>
    /// <returns></returns>
    bool ShowWithStoreFilter(ItemType highlightedItemType);
}