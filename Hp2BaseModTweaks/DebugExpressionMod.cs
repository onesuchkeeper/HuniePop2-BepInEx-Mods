using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseModTweaks;

public class DebugExpressionMod : DataMod, IBodySubDataMod<GirlExpressionSubDefinition>
{
    private IBodySubDataMod<GirlExpressionSubDefinition> _decorated;

    public DebugExpressionMod(IBodySubDataMod<GirlExpressionSubDefinition> decorated)
    {
        _decorated = decorated;
    }

    public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods() => _decorated.GetPartDataMods();

    public void RequestInternals(AssetProvider assetProvider) => _decorated.RequestInternals(assetProvider);

    public void SetData(GirlExpressionSubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, RelativeId girlId, GirlBodySubDefinition bodyDef)
    {
        _decorated.SetData(def, gameData, assetProvider, girlId, bodyDef);
    }
}