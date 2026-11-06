// Hp2BaseMod 2026, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Save;

namespace Hp2BaseMod;

/// <summary>
/// Clears everything mod related for one save slot, then lets other mods clear theirs.
/// </summary>
internal static class SaveSlotReset
{
    public static void Reset(int fileIndex)
    {
        if (fileIndex < 0) return;

        ModInterface.Log.Message($"Resetting mod data for save file {fileIndex}");

        // GetFile pads the list up to the index, so the replacement below can't go out of range
        ModInterface.Save.GetFile(fileIndex);
        ModInterface.Save.ModFiles[fileIndex] = new();

        ModInterface.Events.NotifySaveFileReset(fileIndex);
    }
}

/// <summary>
/// The base game erases a profile by replacing it, not by calling SaveFile.Reset on the existing file:
///   Game.Persistence.playerData.files[i] = new PlayerFile(new SaveFile());
/// so the SaveFile.Reset hook below never sees an erase. This hooks the erase itself.
/// It is a prefix because it has to queue the pipeline before the original method applies and saves the file.
/// </summary>
[HarmonyPatch(typeof(UiCellphoneAppErase), "OnSaveFileButtonPressed")]
internal static class UiCellphoneAppErase_Patch
{
    [HarmonyPrefix]
    private static void Prefix(UiAppSaveFile saveFile)
    {
        // Same conditions the original uses to decide whether it erases anything
        if (saveFile == null 
            || !saveFile.eraseMode 
            || saveFile.playerFile == null 
            || !saveFile.playerFile.started)
        {
            return;
        }

        ModInterface.SaveManager.QueueReset(saveFile.saveFileIndex);
    }
}

/// <summary>
/// Starting a new game fills the PlayerFile of the chosen slot, applies it and saves. The queued pipeline runs
/// when the file is applied, the same way it does for an erase.
/// </summary>
[HarmonyPatch(typeof(UiCellphoneAppNew), "OnStartButtonPressed")]
internal static class UiCellphoneAppNew_Patch
{
    [HarmonyPrefix]
    private static void Prefix(int ____newSaveFileIndex)
    {
        ModInterface.SaveManager.QueueCreated(____newSaveFileIndex);
    }
}

[HarmonyPatch(typeof(SaveFile))]
internal static class SaveFilePatch
{
    private static readonly FieldInfo f_saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");

    /// <summary>
    /// Only matters if something resets a SaveFile that is already in the save data. The base game doesn't
    /// (its constructor and WriteData call Reset on brand new files, which aren't in the list yet), so this
    /// is guarded against the persistence or its save data not existing yet (e.g. first launch).
    /// </summary>
    [HarmonyPatch(nameof(SaveFile.Reset))]
    [HarmonyPostfix]
    public static void Reset(SaveFile __instance)
    {
        if (Game.Persistence == null) return;

        var saveData = f_saveData.GetValue<SaveData>(Game.Persistence);
        var fileIndex = saveData?.files?.IndexOf(__instance) ?? -1;

        if (fileIndex != -1)
        {
            SaveSlotReset.Reset(fileIndex);
        }
    }
}