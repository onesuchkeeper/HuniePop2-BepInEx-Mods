using System;
using System.Collections.Generic;

namespace SingleDate;

[Serializable]
/// <summary>
/// Everything this mod saves, serialized to JSON into the game's save (see <see cref="State.On_PreGameSave"/>).
/// Holds one <see cref="SingleSaveFile"/> for each of the base game's save slots, by index.
/// </summary>
public class SingleSaveData
{
    /// <summary>The data for each save slot, the index is the base game's save file index.</summary>
    public List<SingleSaveFile> SaveFiles;

    /// <summary>Fixes up data after loading: creates anything missing and removes references to content that no longer exists.</summary>
    public void Clean()
    {
        SaveFiles ??= new List<SingleSaveFile>();
        SaveFiles.ForEach(x => x.Clean());
    }
}
