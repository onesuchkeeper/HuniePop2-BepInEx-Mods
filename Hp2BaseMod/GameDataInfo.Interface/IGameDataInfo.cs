// Hp2BaseMod 2022, By OneSuchKeeper

using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGameDefinitionInfo<T>
    {
        /// <summary>
        /// Writes to the game data definition this represents
        /// </summary>
        void SetData(ref T def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle);

        /// <summary>
        /// Allows the mod an opportunity to request internal assets from the assetProvider 
        /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
        /// </summary>
        void RequestInternals(AssetProvider assetProvider);
    }
}
