using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGirlDataMod : IGameDataMod<GirlDefinition>
    {
        //IEnumerable<(RelativeId, IEnumerable<IDialogLineDataMod>)> GetLinesByDialogTriggerId();
        IEnumerable<IGirlBodyDataMod> GetBodyMods();
    }
}
