using System.Collections.Generic;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

/// <summary>
/// Store handler for items that are never part of the base store pools, but can still be
/// placed in the store by a mod (e.g. quest items injected via the PopulateStoreProducts event).
/// Creates no categories of its own; the mod adding the item owns the category and its pool.
/// </summary>
public sealed class MiscItemStoreHandler : ItemStoreHandler
{
    public override IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile)
    {
        yield break;
    }

    /// <summary>
    /// Misc items are always enabled when the MISC filter is active (handled by the caller),
    /// and are shown dimmed under every other filter.
    /// </summary>
    public override bool ShowWithStoreFilter(ItemType highlightedItemType) => false;
}