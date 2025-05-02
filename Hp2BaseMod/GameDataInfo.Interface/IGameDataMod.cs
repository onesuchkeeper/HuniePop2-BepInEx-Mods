// Hp2BaseMod 2022, By OneSuchKeeper

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGameDataMod<T>
    {
        RelativeId Id { get; }
        int LoadPriority { get; }

        /// <summary>
        /// Writes to the game data definition this represents
        /// </summary>
        /// <param name="def">The target game date definition to write to.</param>
        /// <param name="gameData">The game data.</param>
        /// <param name="assetProvider">The asset provider.</param>
        void SetData(T def, GameDefinitionProvider gameData, AssetProvider assetProvider);

        /// <summary>
        /// Allows the mod an opportunity to request internal assets from the assetProvider 
        /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
        /// </summary>
        void RequestInternals(AssetProvider assetProvider);
    }
}
