using System.Collections.Generic;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IGirlBodyDataMod
{
    RelativeId Id { get; }
    int LoadPriority { get; }

    /// <summary>
    /// Returns all <see cref="IGirlPartDataMod"/> defined by this mod
    /// </summary>
    /// <returns></returns>
    IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods();
    IEnumerable<IBodySubDataMod<GirlHairstyleSubDefinition>> GetHairstyles();
    IEnumerable<IBodySubDataMod<GirlOutfitSubDefinition>> GetOutfits();
    IEnumerable<IBodySubDataMod<GirlExpressionSubDefinition>> GetExpressions();
    IEnumerable<IBodySubDataMod<GirlSpecialPartSubDefinition>> GetSpecialPartMods();

    void SetData(GirlBodySubDefinition def,
        GameDefinitionProvider gameData,
        AssetProvider assetProvider,
        RelativeId girlId);

    /// <summary>
    /// Allows the mod an opportunity to request internal assets from the assetProvider 
    /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
    /// </summary>
    void RequestInternals(AssetProvider assetProvider);
}