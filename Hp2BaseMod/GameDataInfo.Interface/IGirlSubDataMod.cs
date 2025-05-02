using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGirlSubDataMod<T>
    {
        RelativeId Id { get; }
        int LoadPriority { get; }

        /// <summary>
        /// Writes to the definition this modifies
        /// </summary>
        void SetData(ref T def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            InsertStyle insertStyle,
            RelativeId girlId);

        /// <summary>
        /// Allows the mod an opportunity to request internal assets from the assetProvider 
        /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
        /// </summary>
        void RequestInternals(AssetProvider assetProvider);
    }
}
