using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

public class DataModRegistry
{
    #region GameDataMods

    internal IEnumerable<IGameDataMod<AbilityDefinition>> AbilityDataMods => _abilityDataMods;
    private List<IGameDataMod<AbilityDefinition>> _abilityDataMods = new List<IGameDataMod<AbilityDefinition>>();

    internal IEnumerable<IGameDataMod<AilmentDefinition>> AilmentDataMods => _ailmentDataMods;
    private List<IGameDataMod<AilmentDefinition>> _ailmentDataMods = new List<IGameDataMod<AilmentDefinition>>();

    internal IEnumerable<IGameDataMod<CodeDefinition>> CodeDataMods => _codeDataMods;
    private List<IGameDataMod<CodeDefinition>> _codeDataMods = new List<IGameDataMod<CodeDefinition>>();

    internal IEnumerable<IGameDataMod<CutsceneDefinition>> CutsceneDataMods => _cutsceneDataMods;
    private List<IGameDataMod<CutsceneDefinition>> _cutsceneDataMods = new List<IGameDataMod<CutsceneDefinition>>();

    internal IEnumerable<IGameDataMod<DialogTriggerDefinition>> DialogTriggerDataMods => _dialogTriggerDataMods;
    private List<IGameDataMod<DialogTriggerDefinition>> _dialogTriggerDataMods = new List<IGameDataMod<DialogTriggerDefinition>>();

    internal IEnumerable<IGameDataMod<DlcDefinition>> DlcDataMods => _dlcDataMods;
    private List<IGameDataMod<DlcDefinition>> _dlcDataMods = new List<IGameDataMod<DlcDefinition>>();

    internal IEnumerable<IGameDataMod<EnergyDefinition>> EnergyDataMods => _energyDataMods;
    private List<IGameDataMod<EnergyDefinition>> _energyDataMods = new List<IGameDataMod<EnergyDefinition>>();

    internal IEnumerable<IGirlDataMod> GirlDataMods => _girlDataMods;
    private List<IGirlDataMod> _girlDataMods = new List<IGirlDataMod>();

    internal IEnumerable<IGirlPairDataMod> GirlPairDataMods => _girlPairDataMods;
    private List<IGirlPairDataMod> _girlPairDataMods = new List<IGirlPairDataMod>();

    internal IEnumerable<IGameDataMod<ItemDefinition>> ItemDataMods => _itemDataMods;
    private List<IGameDataMod<ItemDefinition>> _itemDataMods = new List<IGameDataMod<ItemDefinition>>();

    internal IEnumerable<IGameDataMod<LocationDefinition>> LocationDataMods => _locationDataMods;
    private List<IGameDataMod<LocationDefinition>> _locationDataMods = new();

    internal IEnumerable<IGameDataMod<PhotoDefinition>> PhotoDataMods => _photoDataMods;
    private List<IGameDataMod<PhotoDefinition>> _photoDataMods = new List<IGameDataMod<PhotoDefinition>>();

    internal IEnumerable<IFavQuestionDataMod> QuestionDataMods => _questionDataMods;
    private List<IFavQuestionDataMod> _questionDataMods = new();

    internal IEnumerable<IGameDataMod<TokenDefinition>> TokenDataMods => _tokenDataMods;
    private List<IGameDataMod<TokenDefinition>> _tokenDataMods = new List<IGameDataMod<TokenDefinition>>();

    private Dictionary<RelativeId, UiDollSpecialEffect> _dollSpecialEffects = new();

    private Dictionary<RelativeId, IPuzzleResource> _puzzleResources = new();

    private Dictionary<RelativeId, IAffection> _affections = new();

    private Dictionary<RelativeId, IItemGiftHandler> _itemGiftHandlers = new();

    private Dictionary<RelativeId, IItemStoreHandler> _itemStoreHandlers = new();
    
    public IReadOnlyList<IExpInfo> ExpDisplays => _expDisplays;
    private List<IExpInfo> _expDisplays = new();

    #endregion

    private bool _dataModsApplied = false;

    internal bool TryApplyDataMods(out GameDefinitionProvider gameDefinitionProvider)
    {
        if (_dataModsApplied)
        {
            gameDefinitionProvider = null;
            return false;
        }

        _dataModsApplied = true;

        ModInterface.Events.NotifyPreDataMods();
        gameDefinitionProvider = new GameDefinitionProvider(
            Game.Data, 
            _itemStoreHandlers, 
            _itemGiftHandlers, 
            _affections, 
            _puzzleResources, 
            _dollSpecialEffects,
            _expDisplays);
        GameDataModder.Mod(Game.Data, gameDefinitionProvider);

        _dlcDataMods = null;
        _codeDataMods = null;
        _girlDataMods = null;
        _itemDataMods = null;
        _photoDataMods = null;
        _tokenDataMods = null;
        _energyDataMods = null;
        _energyDataMods = null;
        _abilityDataMods = null;
        _ailmentDataMods = null;
        _cutsceneDataMods = null;
        _girlPairDataMods = null;
        _locationDataMods = null;
        _questionDataMods = null;
        _dialogTriggerDataMods = null;
        _dollSpecialEffects = null;
        _puzzleResources = null;
        _affections = null;
        _itemGiftHandlers = null;
        _itemStoreHandlers = null;
        _expDisplays = null;

        GC.Collect();
        return true;
    }

    public void AddExp(IExpInfo expDisplay)
    {
        _expDisplays.Add(expDisplay);
    }

    public void AddDataMod(IGameDataMod<AbilityDefinition> mod)
    {
        if (mod == null) return;
        _abilityDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Ability, mod.Id);
    }

    public void AddDataMod(IGameDataMod<AilmentDefinition> mod)
    {
        if (mod == null) return;
        _ailmentDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Ailment, mod.Id);
    }

    public void AddDataMod(IGameDataMod<CodeDefinition> mod)
    {
        if (mod == null) return;
        _codeDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Code, mod.Id);
    }

    public void AddDataMod(IGameDataMod<CutsceneDefinition> mod)
    {
        if (mod == null) return;
        _cutsceneDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Cutscene, mod.Id);
    }

    public void AddDataMod(IGameDataMod<DialogTriggerDefinition> mod)
    {
        if (mod == null) return;
        _dialogTriggerDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.DialogTrigger, mod.Id);
    }

    public void AddDataMod(IGameDataMod<DlcDefinition> mod)
    {
        if (mod == null) return;
        _dlcDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Dlc, mod.Id);
    }

    public void AddDataMod(IGameDataMod<EnergyDefinition> mod)
    {
        if (mod == null) return;
        _energyDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Energy, mod.Id);
    }

    public void AddDataMod(IGirlDataMod mod)
    {
        if (mod == null) return;
        _girlDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Girl, mod.Id);
    }
    public void AddDataMod(IGirlPairDataMod mod)
    {
        if (mod == null) return;
        _girlPairDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.GirlPair, mod.Id);
    }
    public void AddDataMod(IGameDataMod<ItemDefinition> mod)
    {
        if (mod == null) return;
        _itemDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Item, mod.Id);
    }
    public void AddDataMod(IGameDataMod<LocationDefinition> mod)
    {
        if (mod == null) return;
        _locationDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Location, mod.Id);
    }
    public void AddDataMod(IGameDataMod<PhotoDefinition> mod)
    {
        if (mod == null) return;
        _photoDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Photo, mod.Id);
    }
    public void AddDataMod(IFavQuestionDataMod mod)
    {
        if (mod == null) return;
        _questionDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Question, mod.Id);
    }
    public void AddDataMod(IGameDataMod<TokenDefinition> mod)
    {
        if (mod == null) return;
        _tokenDataMods.Add(mod);
        ModInterface.Data.TryRegisterDataId(GameDataType.Token, mod.Id);
    }
    public void AddData(RelativeId id, UiDollSpecialEffect specialEffect)
    {
        if (specialEffect == null) return;
        _dollSpecialEffects[id] = specialEffect;
    }
    public void AddData(RelativeId id, IPuzzleResource puzzleResource)
    {
        if (puzzleResource == null) return;
        _puzzleResources[id] = puzzleResource;
    }
    public void AddData(RelativeId id, IAffection affection)
    {
        if (affection == null) return;
        _affections[id] = affection;
    }
    public void AddData(RelativeId id, IItemGiftHandler itemGiftHandler)
    {
        if (itemGiftHandler == null) return;
        _itemGiftHandlers[id] = itemGiftHandler;
    }
}
