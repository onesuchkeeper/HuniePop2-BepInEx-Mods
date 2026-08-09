using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public sealed class DateGiftStoreHandler : ItemStoreHandler
{
    public const int Priority = 2_000_000;
    public const int TargetCount = 12;

    public override IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile)
    {
        yield return new(
            ItemTypes.DateGift,
            new Category<ExpandedItemDefinition>
            {
                TargetCount = TargetCount,
                Priority = Priority,
                Pool = Game.Data.Items.GetAll().Select(x => x.GetExpansion())
                    .Where(x => x.StoreHandler == this)
                    .Where(x =>
                        !playerFile.IsItemInInventory(x.Core, true, 2) &&
                        (!x.Core.difficultyExclusive ||
                         x.Core.difficulty == playerFile.settingDifficulty))
                    .Select(x => new Category<ExpandedItemDefinition>.Entry(x, 1))
                    .ToList()
            });
    }

    public override bool ShowWithStoreFilter(ItemType highlightedItemType) 
        => highlightedItemType == ItemType.DATE_GIFT;
}