using Hp2BaseMod;

public interface IDialogLineDataMod
{
    public RelativeId Id { get; }
    public int LoadPriority { get; }

    /// <summary>
    /// Allows the mod an opportunity to request internal assets from the assetProvider 
    /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
    /// </summary>
    void RequestInternals(AssetProvider assetProvider);

    public void SetData(DialogLine def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider);
}