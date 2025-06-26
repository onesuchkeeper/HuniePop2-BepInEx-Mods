using System;
using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGirlDataMod : IGameDataMod<GirlDefinition>
    {
        IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartMods();
        IEnumerable<IGirlSubDataMod<GirlHairstyleSubDefinition>> GetHairstyles();
        IEnumerable<IGirlSubDataMod<GirlOutfitSubDefinition>> GetOutfits();
        IEnumerable<IGirlSubDataMod<GirlExpressionSubDefinition>> GetExpressions();
        IEnumerable<Tuple<RelativeId, IEnumerable<IGirlSubDataMod<DialogLine>>>> GetLinesByDialogTriggerId();
    }
}
