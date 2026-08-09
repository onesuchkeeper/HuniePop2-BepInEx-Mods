using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public sealed class FoodStoreHandler : ItemStoreHandler
{
    public const int FoodPriority = int.MaxValue;
    public const int StaminaPriority = 3_000_000;

    public int StaminaFoodTargetCount = 4;

    public override IEnumerable<KeyValuePair<RelativeId, Category<ExpandedItemDefinition>>> CreateStoreCategories(PlayerFile playerFile)
    {
        var foods = Game.Data.Items.GetAll().Select(x => x.GetExpansion()).Where(x => x.StoreHandler == this);

        yield return new(
            ItemTypes.Food,
            new Category<ExpandedItemDefinition>
            {
                TargetCount = int.MaxValue,
                Priority = FoodPriority,
                Pool = foods
                    .Where(x => !x.Core.noStaminaCost)
                    .Select(x => new Category<ExpandedItemDefinition>.Entry(x, 1))
                    .ToList()
            });

        yield return new(
            ItemTypes.StaminaFood,
            new Category<ExpandedItemDefinition>
            {
                TargetCount = StaminaFoodTargetCount,
                Priority = StaminaPriority,
                Pool = foods
                    .Where(x => x.Core.noStaminaCost)
                    .Select(x => new Category<ExpandedItemDefinition>.Entry(x, 1))
                    .ToList()
            });
    }

    public override bool ShowWithStoreFilter(ItemType highlightedItemType) 
        => highlightedItemType == ItemType.FOOD;
}
