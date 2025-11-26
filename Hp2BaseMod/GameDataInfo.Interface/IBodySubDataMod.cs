using System.Collections.Generic;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IBodySubDataMod<T>
    {
        RelativeId Id { get; }
        int LoadPriority { get; }

        /// <summary>
        /// Writes to the definition this modifies
        /// </summary>
        void SetData(T def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            RelativeId girlId,
            GirlBodySubDefinition bodyDef);

        /// <summary>
        /// Returns all <see cref="IGirlPartDataMod"/> defined by this mod
        /// </summary>
        /// <returns></returns>
        IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods();

        /// <summary>
        /// Allows the mod an opportunity to request internal assets from the assetProvider 
        /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
        /// </summary>
        void RequestInternals(AssetProvider assetProvider);
    }
}
