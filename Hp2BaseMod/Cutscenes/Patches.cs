// using System;
// using HarmonyLib;
// using Hp2BaseMod.Utility;

// namespace Hp2BaseMod;

// [HarmonyPatch(typeof(CutsceneManager), "StartCutscene")]
// public static class Patches
// {
//     public static void Prefix(CutsceneDefinition cutsceneDef, CutsceneDefinition innerCutsceneDef)
//     {
//         ModInterface.Log.LogInfo($"Starting cutscene: {cutsceneDef.name}");

//         using (ModInterface.Log.MakeIndent())
//         {
//             GameDataLogUtility.LogCutscene(cutsceneDef);
//         }
//     }
// }