// Hp2BaseMod 2022, By OneSuchKeeper
using HarmonyLib;

namespace Hp2BaseMod
{
    /// <summary>
    /// source code uses a bunch of enums for the names exclusively, but those can't be expanded, so default to using the misc "category description"
    /// for everything instead
    /// </summary>
    [HarmonyPatch(typeof(ItemDefinition), "GetCategoryName")]
    public static class ItemDefinition_GetCategoryNamePatch
    {
        public static bool Prefix(ItemDefinition __instance, ref string __result)
        {
            // prioritize category description over all else
            if (!string.IsNullOrWhiteSpace(__instance.categoryDescription))
            {
                // TODO, make a full system for handling addition types
                // for not just make negatives misc
                var typeStr = (int)__instance.itemType < 0
                    ? "Misc"
                    : __instance.itemType.ToString();

                __result = StringUtils.Titleize(typeStr) + " • " + __instance.categoryDescription;
                return false;
            }

            return true;
        }
    }
}
