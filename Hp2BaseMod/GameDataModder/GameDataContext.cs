using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

internal class GameDataContext
{
    public Dictionary<int, AbilityDefinition> abilityDataDict;
    public IEnumerable<IGameDataMod<AbilityDefinition>> abilityDataMods;

    public Dictionary<int, AilmentDefinition> ailmentDataDict;
    public IEnumerable<IGameDataMod<AilmentDefinition>> ailmentDataMods;

    public Dictionary<int, CodeDefinition> codeDataDict;
    public IEnumerable<IGameDataMod<CodeDefinition>> codeDataMods;

    public Dictionary<int, CutsceneDefinition> cutsceneDataDict;
    public IEnumerable<IGameDataMod<CutsceneDefinition>> cutsceneDataMods;

    public Dictionary<int, DialogTriggerDefinition> dialogTriggerDataDict;
    public IEnumerable<IGameDataMod<DialogTriggerDefinition>> dialogTriggerDataMods;

    public Dictionary<int, DlcDefinition> dlcDataDict;
    public IEnumerable<IGameDataMod<DlcDefinition>> dlcDataMods;

    public Dictionary<int, EnergyDefinition> energyDataDict;
    public IEnumerable<IGameDataMod<EnergyDefinition>> energyDataMods;

    public Dictionary<int, GirlDefinition> girlDataDict;
    public IEnumerable<IGirlDataMod> girlDataMods;

    public Dictionary<int, GirlPairDefinition> girlPairDataDict;
    public IEnumerable<IGirlPairDataMod> girlPairDataMods;

    public Dictionary<int, ItemDefinition> itemDataDict;
    public IEnumerable<IGameDataMod<ItemDefinition>> itemDataMods;

    public Dictionary<int, LocationDefinition> locationDataDict;
    public IEnumerable<IGameDataMod<LocationDefinition>> locationDataMods;

    public Dictionary<int, PhotoDefinition> photoDataDict;
    public IEnumerable<IGameDataMod<PhotoDefinition>> photoDataMods;

    public Dictionary<int, QuestionDefinition> questionDataDict;
    public IEnumerable<IFavQuestionDataMod> questionDataMods;

    public Dictionary<int, TokenDefinition> tokenDataDict;
    public IEnumerable<IGameDataMod<TokenDefinition>> tokenDataMods;
}