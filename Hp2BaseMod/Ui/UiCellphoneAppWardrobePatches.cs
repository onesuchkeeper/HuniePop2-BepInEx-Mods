using HarmonyLib;

namespace Hp2BaseMod.Ui
{
    [HarmonyPatch(typeof(UiCellphoneAppWardrobe))]
    public static class UiCellphoneAppWardrobePatches
    {
        // [HarmonyPrefix]
        // [HarmonyPatch("Refresh")]
        // public static void PreRefresh(UiCellphoneAppWardrobe __instance)
        // {
        //     var manager = ModInterface.Ui.GetCellphoneManager(__instance.cellphone);

        //     if (manager == null)
        //     {
        //         return;
        //     }

        //     ModInterface.Log.LogInfo("Wardrobe pre refresh");
        //     manager.PreRefresh();
        // }

        // [HarmonyPostfix]
        // [HarmonyPatch("Refresh")]
        // public static void PostRefresh(UiCellphoneAppWardrobe __instance)
        // {
        //     ModInterface.Log.LogInfo("Wardrobe post refresh");
        //     var manager = ModInterface.Ui.GetCellphoneManager(__instance.cellphone);

        //     if (manager == null)
        //     {
        //         return;
        //     }

        //     manager.PostRefresh();
        // }
    }
}