// Hp2BaseMod 2026, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod.Save;

/// <summary>
/// Sends every save file through a <see cref="SavePipeline"/>. Loading hands its files over directly.
/// The new game and erase screens queue a pipeline, which runs when the base game applies the file,
/// that is after it was written and before it is saved.
/// </summary>
internal class SaveManager
{
    private readonly FieldInfo f_saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");

    private SavePipeline _queued;

    public void ProcessLoaded(int fileIndex, SaveFile saveFile) => new LoadedSavePipeline(fileIndex).Process(saveFile);

    public void QueueCreated(int fileIndex) => _queued = new CreatedSavePipeline(fileIndex);

    public void QueueReset(int fileIndex) => _queued = new ResetSavePipeline(fileIndex);

    /// <summary>
    /// Called when the base game wrote a PlayerFile into the save data. Does nothing unless a pipeline is queued for that file.
    /// </summary>
    public void OnFileApplied(int fileIndex)
    {
        if (_queued?.FileIndex != fileIndex) return;

        var pipeline = _queued;
        _queued = null;

        var saveFile = ((SaveData)f_saveData.GetValue(Game.Persistence)).files[fileIndex];
        pipeline.Process(saveFile);

        // The profile screens keep using the PlayerFile in memory. A loaded game rebuilds all of them from the
        // cleaned save data, so do the same here or the screens would show the file as it was before cleaning.
        Game.Persistence.playerData.files[fileIndex] = new PlayerFile(saveFile);
    }
}