using HarmonyLib;

namespace Hp2BaseMod;

[Expansion(typeof(ItemDefinition), HasModId = true)]
#pragma warning disable HP001 // Deprecated member usage
[Deprecates(nameof(ItemDefinition.itemType), $"Use a {nameof(ExpandedItemDefinition)}.{nameof(ExpandedItemDefinition.GiftHandler)} or {nameof(ExpandedItemDefinition)}.{nameof(ExpandedItemDefinition.StoreHandler)}")]
[Deprecates(nameof(ItemDefinition.affectionType), $"Use {nameof(ExpandedItemDefinition)}.{nameof(ExpandedItemDefinition.Affection)}")]
#pragma warning restore HP001 // Deprecated member usage
public partial class ExpandedItemDefinition
{
    [HarmonyPatch(typeof(ItemDefinition), "GetCategoryName")]
    private static class Patch
    {
        [HarmonyPatch("GetCategoryName")]
        [HarmonyPrefix]
        private static bool GetCategoryName(ItemDefinition __instance, ref string __result) 
            => ExpandedItemDefinition.Get(__instance).GetCategoryName_Prefix(ref __result);
    }

    public IAffection Affection;
    public IItemGiftHandler GiftHandler;
    public IItemStoreHandler StoreHandler;

    /// <summary>
    /// Custom category prefix (e.g. "Smoothie", "Food", "Shoe").
    /// Replaces reading _core.itemType.
    /// </summary>
    public string CategoryPrefix;

    /// <summary>
    /// Indicates whether this item decays over time in inventory.
    /// Replaces checking itemType == ItemType.SMOOTHIE || itemType == ItemType.FOOD.
    /// </summary>
    public bool IsPerishable;

    public bool GetCategoryName_Prefix(ref string __result)
    {
        if (!string.IsNullOrWhiteSpace(_core.categoryDescription))
        {
            var prefix = !string.IsNullOrWhiteSpace(CategoryPrefix) 
                ? CategoryPrefix 
                : ((int)_core.itemType < 0 ? "Misc" : StringUtils.Titleize(_core.itemType.ToString()));

            __result = $"{prefix} • {_core.categoryDescription}";
            ModInterface.Log.Message($"Custom item category description: {__result}");
            return false;
        }

        return true;
    }
}
