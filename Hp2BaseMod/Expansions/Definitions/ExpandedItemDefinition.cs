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
        private static void GetCategoryName(ItemDefinition __instance, ref string __result) 
            => ExpandedItemDefinition.Get(__instance).GetCategoryName_Prefix(ref __result);
    }

    public IAffection Affection;
    public IItemGiftHandler GiftHandler;
    public IItemStoreHandler StoreHandler;

    public bool GetCategoryName_Prefix(ref string __result)
    {
        // prioritize category description over all else
        if (!string.IsNullOrWhiteSpace(_core.categoryDescription))
        {
            // TODO, make a full system for handling addition types
            // for now just make negatives misc
            var typeStr = (int)_core.itemType < 0
                ? "Misc"
                : _core.itemType.ToString();

            __result = StringUtils.Titleize(typeStr) + " • " + _core.categoryDescription;
            return false;
        }

        return true;
    }
}
