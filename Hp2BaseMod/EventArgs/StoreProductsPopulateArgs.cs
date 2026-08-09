using System.Collections.Generic;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public class StoreProductsPopulateArgs
{
    public Dictionary<RelativeId, Category<ExpandedItemDefinition>> ItemCategories => _itemCategories;
    private Dictionary<RelativeId, Category<ExpandedItemDefinition>> _itemCategories = new();
}