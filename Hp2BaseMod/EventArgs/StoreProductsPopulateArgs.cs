using System.Collections.Generic;
using Hp2BaseMod.Elements;

namespace Hp2BaseMod;

public class StoreProductsPopulateArgs
{
    public Dictionary<RelativeId, Category<ItemDefinition>> ItemCategories => _itemCategories;
    private Dictionary<RelativeId, Category<ItemDefinition>> _itemCategories = new();
}