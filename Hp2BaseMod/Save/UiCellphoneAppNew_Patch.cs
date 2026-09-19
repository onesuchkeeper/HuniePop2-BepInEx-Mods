// using System.Reflection;
// using HarmonyLib;
// using Hp2BaseMod.Extension;

// namespace Hp2BaseMod;

// [HarmonyPatch(typeof(UiCellphoneAppNew), "OnStartButtonPressed")]
// public static class UiCellphoneAppNew_Patch
// {
//     private static FieldInfo f_newSaveFileIndex = AccessTools.Field(typeof(UiCellphoneAppNew), "_newSaveFileIndex");

//     public static void Prefix(UiTitleCanvas __instance, ButtonBehavior buttonBehavior)
//     {
//         //This needs to be done here and not on load, because if the player erases their save it needs to be set
//         //
//         ModInterface.Log.Message("Initializing new profile with special state.");
//         ModInterface.Save.GetFile(f_newSaveFileIndex.GetValue<int>(__instance)).GameStateId = GameStateId.Special;
//     }
// }
