// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

/// <summary>
/// Wrapper of <see cref="GameData"/> to handle nullable and relative ids in looking up game data
/// </summary>
public class GameDefinitionProvider
{
    private readonly GameData _gameData;

    public Dictionary<RelativeId, UiDollSpecialEffect> DollSpecialEffects => _dollSpecialEffects;
    internal readonly Dictionary<RelativeId, UiDollSpecialEffect> _dollSpecialEffects;

    public Dictionary<RelativeId, IPuzzleResource> PuzzleResources => _puzzleResources;
    internal readonly Dictionary<RelativeId, IPuzzleResource> _puzzleResources;

    public Dictionary<RelativeId, ITokenHandler> TokenHandlers => _tokenHandlers;
    internal readonly Dictionary<RelativeId, ITokenHandler> _tokenHandlers;

    public IReadOnlyDictionary<RelativeId, IAffection> Affections => _affections;
    internal readonly Dictionary<RelativeId, IAffection> _affections;

    public IReadOnlyDictionary<RelativeId, IItemGiftHandler> ItemGiftHandlers => _itemGiftHandlers;
    internal readonly Dictionary<RelativeId, IItemGiftHandler> _itemGiftHandlers;

    public IReadOnlyDictionary<RelativeId, IItemStoreHandler> ItemStoreHandlers => _itemStoreHandlers;
    internal readonly Dictionary<RelativeId, IItemStoreHandler> _itemStoreHandlers;

    public IReadOnlyDictionary<RelativeId, IPuzzleStatusGirlState> PuzzleStatusGirlStates => _puzzleStatusGirlStates;
    internal readonly Dictionary<RelativeId, IPuzzleStatusGirlState> _puzzleStatusGirlStates;

    public IReadOnlyList<IExpInfo> ExpDisplays => _expDisplays;
    internal readonly List<IExpInfo> _expDisplays = new List<IExpInfo>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gameData"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public GameDefinitionProvider(GameData gameData,
        Dictionary<RelativeId, IItemStoreHandler> itemStoreHandlers,
        Dictionary<RelativeId, IItemGiftHandler> itemGiftHandlers,
        Dictionary<RelativeId, IAffection> affections,
        Dictionary<RelativeId, IPuzzleResource> puzzleResources,
        Dictionary<RelativeId, ITokenHandler> tokenHandlers,
        Dictionary<RelativeId, IPuzzleStatusGirlState> puzzleStatusGirlStates,
        Dictionary<RelativeId, UiDollSpecialEffect> dollSpecialEffects,
        List<IExpInfo> expDisplays)
    {
        _itemStoreHandlers = itemStoreHandlers;
        _itemGiftHandlers = itemGiftHandlers;
        _affections = affections;
        _puzzleResources = puzzleResources;
        _tokenHandlers = tokenHandlers;
        _puzzleStatusGirlStates = puzzleStatusGirlStates;
        _dollSpecialEffects = dollSpecialEffects;
        _expDisplays = expDisplays;
        _gameData = gameData ?? throw new ArgumentNullException(nameof(gameData));
    }

    internal void AddDollSpecialEffect(RelativeId id, UiDollSpecialEffect uiDollSpecialEffect)
    {
        _dollSpecialEffects[id] = uiDollSpecialEffect;
    }

    internal void AddPuzzleResource(RelativeId id, IPuzzleResource puzzleResource)
    {
        _puzzleResources[id] = puzzleResource;
    }

    internal void AddAffection(RelativeId id, IAffection affection)
    {
        _affections[id] = affection;
    }

    internal void AddItemGiftHandler(RelativeId id, IItemGiftHandler itemGiftHandler)
    {
        _itemGiftHandlers[id] = itemGiftHandler;
    }

    /// <summary>
    /// Gets the <see cref="Definition"/> for the given id. Returns null if one doesn't exist.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns>The <see cref="Definition"/> for the given id. Returns null if one doesn't exist.</returns>
    public Definition GetDefinition(GameDataType type, RelativeId? id) => id.HasValue ? GetDefinition(type, ModInterface.Data.GetRuntimeDataId(type, id.Value)) : null;

    /// <summary>
    /// Gets the <see cref="Definition"/> for the given id. Returns null if one doesn't exist.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns>The <see cref="Definition"/> for the given id. Returns null if one doesn't exist.</returns>
    public Definition GetDefinition(GameDataType type, RelativeId id) => GetDefinition(type, ModInterface.Data.GetRuntimeDataId(type, id));

    /// <summary>
    /// Gets the <see cref="Definition"/> for the given id. Returns null if one doesn't exist.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="runtimeId"></param>
    /// <returns>The <see cref="Definition"/> for the given id. Returns null if one doesn't exist.</returns>
    public Definition GetDefinition(GameDataType type, int runtimeId)
    {
        if (runtimeId == -1)
        {
            return null;
        }

        switch (type)
        {
            case GameDataType.Ability:
                return _gameData.Abilities.Get(runtimeId);
            case GameDataType.Ailment:
                return _gameData.Ailments.Get(runtimeId);
            case GameDataType.Code:
                return _gameData.Codes.Get(runtimeId);
            case GameDataType.Cutscene:
                return _gameData.Cutscenes.Get(runtimeId);
            case GameDataType.DialogTrigger:
                return _gameData.DialogTriggers.Get(runtimeId);
            case GameDataType.Dlc:
                return _gameData.Dlcs.Get(runtimeId);
            case GameDataType.Energy:
                return _gameData.Energy.Get(runtimeId);
            case GameDataType.Girl:
                return _gameData.Girls.Get(runtimeId);
            case GameDataType.GirlPair:
                return _gameData.GirlPairs.Get(runtimeId);
            case GameDataType.Item:
                return _gameData.Items.Get(runtimeId);
            case GameDataType.Location:
                return _gameData.Locations.Get(runtimeId);
            case GameDataType.Photo:
                return _gameData.Photos.Get(runtimeId);
            case GameDataType.Question:
                return _gameData.Questions.Get(runtimeId);
            case GameDataType.Token:
                return _gameData.Tokens.Get(runtimeId);
            default:
                ModInterface.Log.Message($"Failed to find definition for {type} with runtime id {runtimeId}");
                return null;
        }
    }

    public AbilityDefinition GetAbility(RelativeId id) => _gameData.Abilities.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Ability, id));
    public AbilityDefinition GetAbility(RelativeId? id) => id.HasValue ? _gameData.Abilities.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Ability, id.Value)) : null;
    public AilmentDefinition GetAilment(RelativeId id) => _gameData.Ailments.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Ailment, id));
    public AilmentDefinition GetAilment(RelativeId? id) => id.HasValue ? _gameData.Ailments.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Ailment, id.Value)) : null;
    public CodeDefinition GetCode(RelativeId id) => _gameData.Codes.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Code, id));
    public CodeDefinition GetCode(RelativeId? id) => id.HasValue ? _gameData.Codes.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Code, id.Value)) : null;
    public CutsceneDefinition GetCutscene(RelativeId id) => _gameData.Cutscenes.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Cutscene, id));
    public CutsceneDefinition GetCutscene(RelativeId? id) => id.HasValue ? _gameData.Cutscenes.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Cutscene, id.Value)) : null;
    public DialogTriggerDefinition GetDialogTrigger(RelativeId id) => _gameData.DialogTriggers.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, id));
    public DialogTriggerDefinition GetDialogTrigger(RelativeId? id) => id.HasValue ? _gameData.DialogTriggers.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, id.Value)) : null;
    public DlcDefinition GetDlc(RelativeId id) => _gameData.Dlcs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Dlc, id));
    public DlcDefinition GetDlc(RelativeId? id) => id.HasValue ? _gameData.Dlcs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Dlc, id.Value)) : null;
    public EnergyDefinition GetEnergy(RelativeId id) => _gameData.Energy.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Energy, id));
    public EnergyDefinition GetEnergy(RelativeId? id) => id.HasValue ? _gameData.Energy.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Energy, id.Value)) : null;
    public GirlDefinition GetGirl(RelativeId id) => _gameData.Girls.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, id));
    public GirlDefinition GetGirl(RelativeId? id) => id.HasValue ? _gameData.Girls.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, id.Value)) : null;
    public GirlPairDefinition GetGirlPair(RelativeId id) => _gameData.GirlPairs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.GirlPair, id));
    public GirlPairDefinition GetGirlPair(RelativeId? id) => id.HasValue ? _gameData.GirlPairs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.GirlPair, id.Value)) : null;
    public ItemDefinition GetItem(RelativeId id) => _gameData.Items.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Item, id));
    public ItemDefinition GetItem(RelativeId? id) => id.HasValue ? _gameData.Items.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Item, id.Value)) : null;
    public LocationDefinition GetLocation(RelativeId id) => _gameData.Locations.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Location, id));
    public LocationDefinition GetLocation(RelativeId? id) => id.HasValue ? _gameData.Locations.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Location, id.Value)) : null;
    public PhotoDefinition GetPhoto(RelativeId id) => _gameData.Photos.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Photo, id));
    public PhotoDefinition GetPhoto(RelativeId? id) => id.HasValue ? _gameData.Photos.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Photo, id.Value)) : null;
    public QuestionDefinition GetQuestion(RelativeId id) => _gameData.Questions.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Question, id));
    public QuestionDefinition GetQuestion(RelativeId? id) => id.HasValue ? _gameData.Questions.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Question, id.Value)) : null;
    public QuestionDefinition GetQuestion(int runtimeId) => _gameData.Questions.Get(runtimeId);
    public TokenDefinition GetToken(RelativeId id) => _gameData.Tokens.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Token, id));
    public TokenDefinition GetToken(RelativeId? id) => id.HasValue ? _gameData.Tokens.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Token, id.Value)) : null;

    public UiDollSpecialEffect GetSpecialEffect(RelativeId id) => _dollSpecialEffects.TryGetValue(id, out var value) ? value : null;
    public UiDollSpecialEffect GetSpecialEffect(RelativeId? id) => id.HasValue ? GetSpecialEffect(id.Value) : null;
    public IPuzzleResource GetPuzzleResource(RelativeId id) => _puzzleResources.TryGetValue(id, out var value) ? value : null;
    public IPuzzleResource GetPuzzleResource(RelativeId? id) => id.HasValue ? GetPuzzleResource(id.Value) : null;
    public ITokenHandler GetTokenHandler(RelativeId id) => _tokenHandlers.TryGetValue(id, out var value) ? value : null;
    public ITokenHandler GetTokenHandler(RelativeId? id) => id.HasValue ? GetTokenHandler(id.Value) : null;
    public IAffection GetAffection(RelativeId id) => _affections.TryGetValue(id, out var value) ? value : null;
    public IAffection GetAffection(RelativeId? id) => id.HasValue ? GetAffection(id.Value) : null;
    public IItemGiftHandler GetItemGiftHandler(RelativeId id) => _itemGiftHandlers.TryGetValue(id, out var value) ? value : null;
    public IItemGiftHandler GetItemGiftHandler(RelativeId? id) => id.HasValue ? GetItemGiftHandler(id.Value) : null;
    public IItemStoreHandler GetItemStoreHandler(RelativeId id) => _itemStoreHandlers.TryGetValue(id, out var value) ? value : null;
    public IItemStoreHandler GetItemStoreHandler(RelativeId? id) => id.HasValue ? GetItemStoreHandler(id.Value) : null;
    public IPuzzleStatusGirlState GetPuzzleStatusGirlState(RelativeId id) => _puzzleStatusGirlStates.TryGetValue(id, out var value) ? value : null;
    public IPuzzleStatusGirlState GetPuzzleStatusGirlState(RelativeId? id) => id.HasValue ? GetPuzzleStatusGirlState(id.Value) : null;

    /// <summary>
    /// Checks if the code is unlocked
    /// </summary>
    /// <param name="id">Id of the code.</param>
    /// <returns></returns>
    public bool IsCodeUnlocked(RelativeId? id)
    {
        if (!id.HasValue) return false;

        var runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Code, id);
        return Game.Persistence?.playerData?.unlockedCodes.Any(x => x.id == runtimeId) ?? false;
    }

    internal ItemDefinition GetRandomFruit()
    {
        var pool = _affections.Values.Where(x => x.HasFruit).ToArray();
        return pool.GetRandom().GetRandomFruit();
    }
}
