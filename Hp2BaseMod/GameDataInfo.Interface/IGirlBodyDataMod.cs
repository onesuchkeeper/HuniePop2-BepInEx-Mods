using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IGirlBodyDataMod : IGirlSubDataMod<GirlBodySubDefinition>
{
    IEnumerable<IGirlSubDataMod<GirlHairstyleSubDefinition>> GetHairstyles();
    IEnumerable<IGirlSubDataMod<GirlOutfitSubDefinition>> GetOutfits();
    IEnumerable<IGirlSubDataMod<GirlExpressionSubDefinition>> GetExpressions();
    IEnumerable<IGirlSubDataMod<GirlSpecialPartSubDefinition>> GetSpecialPartMods();
}