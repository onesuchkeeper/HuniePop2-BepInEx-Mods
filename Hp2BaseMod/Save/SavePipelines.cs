// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod.Save;

/// <summary>
/// A file read from disk, the mod data it is injected with is what was loaded from the mod save
/// </summary>
internal class LoadedSavePipeline : SavePipeline
{
    public LoadedSavePipeline(int fileIndex) : base(fileIndex) { }
}

/// <summary>
/// A file the base game just made in memory, so the file is the truth and whatever mod data the slot still holds is stale.
/// The slot is cleared first, otherwise injecting would write that stale data over the new file. (An unstarted slot
/// keeps a stripped location and icon of 0, which would replace a new game's starting location with nothing.)
/// </summary>
internal abstract class FreshSavePipeline : SavePipeline
{
    protected FreshSavePipeline(int fileIndex) : base(fileIndex) { }

    protected override void Prepare()
    {
        SaveSlotReset.Reset(FileIndex);
    }
}

/// <summary>
/// A started file from the new game screen. A new game starts in the special state.
/// </summary>
internal class CreatedSavePipeline : FreshSavePipeline
{
    public CreatedSavePipeline(int fileIndex) : base(fileIndex) { }

    protected override void Prepare()
    {
        base.Prepare();
        ModInterface.Save.GetFile(FileIndex).GameStatePayload = new GameStateSpecial.Payload(true);
    }
}

/// <summary>
/// A blank file from erasing a profile
/// </summary>
internal class ResetSavePipeline : FreshSavePipeline
{
    public ResetSavePipeline(int fileIndex) : base(fileIndex) { }
}