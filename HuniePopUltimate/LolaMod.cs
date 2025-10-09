using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;

namespace HuniePopUltimate;

public class LolaMod : IGirlDataMod
{
    private GirlDataMod _decorated;
    public LolaMod(GirlDataMod decorated)
    {
        _decorated = decorated ?? throw new ArgumentNullException();
    }

    public RelativeId Id => _decorated.Id;

    public int LoadPriority => _decorated.LoadPriority;

    public IEnumerable<IGirlBodyDataMod> GetBodyMods() => _decorated.GetBodyMods();

    public IEnumerable<(RelativeId, IEnumerable<IGirlSubDataMod<DialogLine>>)> GetLinesByDialogTriggerId() => _decorated.GetLinesByDialogTriggerId();

    public void RequestInternals(AssetProvider assetProvider) => _decorated.RequestInternals(assetProvider);

    public void SetData(GirlDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider)
    {
        def.girlNickName = null;
        _decorated.SetData(def, gameData, assetProvider);
    }
}