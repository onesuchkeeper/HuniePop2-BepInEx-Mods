using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

internal class BodyData
{
    public List<IGirlBodyDataMod> bodyMods = new();
    public Dictionary<RelativeId, List<IBodySubDataMod<GirlPartSubDefinition>>> partMods = new();
    public Dictionary<RelativeId, List<IBodySubDataMod<GirlOutfitSubDefinition>>> outfitMods = new();
    public Dictionary<RelativeId, List<IBodySubDataMod<GirlHairstyleSubDefinition>>> hairstyleMods = new();
    public Dictionary<RelativeId, List<IBodySubDataMod<GirlSpecialPartSubDefinition>>> specialPartMods = new();
    public Dictionary<RelativeId, List<IBodySubDataMod<GirlExpressionSubDefinition>>> expressionMods = new();
}
