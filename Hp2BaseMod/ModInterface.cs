using System;
using System.Collections.Generic;
using System.IO;
using Hp2BaseMod.Commands;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Save;
using Newtonsoft.Json;
using Sourceage.Element;
using UnityEngine;

namespace Hp2BaseMod;

public static class ModInterface
{
    #region events

    public static event Action PreGameSave;
    internal static void NotifyPreSave() => PreGameSave?.Invoke();

    public static event Action PostGameSave;
    internal static void NotifyPostSave() => PostGameSave?.Invoke();

    public static event Action<SaveFile> PreLoadSaveFile;
    internal static void NotifyPreLoadSaveFile(SaveFile file) => PreLoadSaveFile?.Invoke(file);

    public static event Action PreDataMods;
    public static event Action PostDataMods;

    /// <summary>
    /// Notifies when a girl's style will potentially change, allowing it to be modified.
    /// </summary>
    public static event EventHandler<RequestStyleChangeEventArgs> RequestStyleChange;
    internal static RequestStyleChangeEventArgs NotifyRequestStyleChange(GirlDefinition girl, LocationDefinition loc, float percentage, GirlStyleInfo style)
    {
        var args = new RequestStyleChangeEventArgs(girl, loc, percentage, style);
        RequestStyleChange?.Invoke(null, args);
        return args;
    }

    /// <summary>
    /// Triggers before SaveData is applied to the persistence, allowing for modifications
    /// like the unlocking of styles etc.
    /// </summary>
    public static event Action<SaveData> PrePersistenceReset;
    internal static void NotifyPrePersistenceReset(SaveData playerData) => PrePersistenceReset?.Invoke(playerData);

    public static event Action PostPersistenceReset;
    internal static void NotifyPostPersistenceReset() => PostPersistenceReset?.Invoke();

    #endregion

    private static string _modSavePath = Path.Combine(Application.persistentDataPath, @"ModSaveData.json");

    #region GameDataMods

    public static IEnumerable<IGameDataMod<AbilityDefinition>> AbilityDataMods => _abilityDataMods;
    private static List<IGameDataMod<AbilityDefinition>> _abilityDataMods = new List<IGameDataMod<AbilityDefinition>>();

    public static IEnumerable<IGameDataMod<AilmentDefinition>> AilmentDataMods => _ailmentDataMods;
    private static List<IGameDataMod<AilmentDefinition>> _ailmentDataMods = new List<IGameDataMod<AilmentDefinition>>();

    public static IEnumerable<IGameDataMod<CodeDefinition>> CodeDataMods => _codeDataMods;
    private static List<IGameDataMod<CodeDefinition>> _codeDataMods = new List<IGameDataMod<CodeDefinition>>();

    public static IEnumerable<IGameDataMod<CutsceneDefinition>> CutsceneDataMods => _cutsceneDataMods;
    private static List<IGameDataMod<CutsceneDefinition>> _cutsceneDataMods = new List<IGameDataMod<CutsceneDefinition>>();

    public static IEnumerable<IGameDataMod<DialogTriggerDefinition>> DialogTriggerDataMods => _dialogTriggerDataMods;
    private static List<IGameDataMod<DialogTriggerDefinition>> _dialogTriggerDataMods = new List<IGameDataMod<DialogTriggerDefinition>>();

    public static IEnumerable<IGameDataMod<DlcDefinition>> DlcDataMods => _dlcDataMods;
    private static List<IGameDataMod<DlcDefinition>> _dlcDataMods = new List<IGameDataMod<DlcDefinition>>();

    public static IEnumerable<IGameDataMod<EnergyDefinition>> EnergyDataMods => _energyDataMods;
    private static List<IGameDataMod<EnergyDefinition>> _energyDataMods = new List<IGameDataMod<EnergyDefinition>>();

    public static IEnumerable<IGirlDataMod> GirlDataMods => _girlDataMods;
    private static List<IGirlDataMod> _girlDataMods = new List<IGirlDataMod>();

    public static IEnumerable<IGirlPairDataMod> GirlPairDataMods => _girlPairDataMods;
    private static List<IGirlPairDataMod> _girlPairDataMods = new List<IGirlPairDataMod>();

    public static IEnumerable<IGameDataMod<ItemDefinition>> ItemDataMods => _itemDataMods;
    private static List<IGameDataMod<ItemDefinition>> _itemDataMods = new List<IGameDataMod<ItemDefinition>>();

    public static IEnumerable<ILocationDataMod> LocationDataMods => _locationDataMods;
    private static List<ILocationDataMod> _locationDataMods = new List<ILocationDataMod>();

    public static IEnumerable<IGameDataMod<PhotoDefinition>> PhotoDataMods => _photoDataMods;
    private static List<IGameDataMod<PhotoDefinition>> _photoDataMods = new List<IGameDataMod<PhotoDefinition>>();

    public static IEnumerable<IGameDataMod<QuestionDefinition>> QuestionDataMods => _questionDataMods;
    private static List<IGameDataMod<QuestionDefinition>> _questionDataMods = new List<IGameDataMod<QuestionDefinition>>();

    public static IEnumerable<IGameDataMod<TokenDefinition>> TokenDataMods => _tokenDataMods;
    private static List<IGameDataMod<TokenDefinition>> _tokenDataMods = new List<IGameDataMod<TokenDefinition>>();

    #endregion

    public static IReadOnlyDictionary<string, ICommand> Commands => _commands;
    private static Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

    /// <summary>
    /// The cellphone managers
    /// </summary>
    public static ModUi Ui => _cellphone;
    private static readonly ModUi _cellphone = new ModUi();

    /// <summary>
    /// The session's log
    /// </summary>
    public static ModLog Log => _log;
    private static ModLog _log;

    /// <summary>
    /// Meta information about modded data
    /// </summary>
    public static ModData Data => _data;
    private static ModData _data;

    /// <summary>
    /// State variables exposed to allow for tweaks
    /// </summary>
    public static ModState State => _state;
    private static ModState _state = new ModState();

    /// <summary>
    /// Holds references to requested in-game assets
    /// </summary>
    public static AssetProvider Assets => _assetProvider;
    private static AssetProvider _assetProvider = new AssetProvider();

    private static SetManager<int> _idPool;
    private static Dictionary<int, string> _sourceId_GUID;
    private static ModSaveData _modSaveData;

    internal static void Init()
    {
        _log = new ModLog("Hp2BaseMod");

        _data = new ModData();

        //load in id cache
        if (File.Exists(_modSavePath))
        {
            _modSaveData = JsonConvert.DeserializeObject<ModSaveData>(File.ReadAllText(_modSavePath));
            if (_modSaveData == null)
            {
                Log.LogError("Failed to load mod save data");
                _modSaveData = new ModSaveData();
            }
        }
        else
        {
            _modSaveData = new ModSaveData();
        }

        _idPool = new SetManager<int>(new IntOpHandler(), _modSaveData.SourceGUID_Id.Values);

        _sourceId_GUID = new Dictionary<int, string>();
        foreach (var guid_id in _modSaveData.SourceGUID_Id)
        {
            _sourceId_GUID[guid_id.Value] = guid_id.Key;
        }
    }

    /// <summary>
    /// Strips modded data from the saveData and saves it separately
    /// </summary>
    /// <param name="saveData"></param>
    internal static void StripSave(SaveData saveData)
    {
        _modSaveData.Strip(saveData);
        File.WriteAllText(_modSavePath, JsonConvert.SerializeObject(_modSaveData));
        Log.LogInfo($"Mod data saved");
    }

    internal static void InjectSave(SaveData saveData) => _modSaveData.SetData(saveData);

    private static bool _dataModsApplied = false;
    internal static void ApplyDataMods()
    {
        if (!_dataModsApplied)
        {
            _dataModsApplied = true;

            PreDataMods?.Invoke();
            GameDataModder.Mod(Game.Data);
            PostDataMods?.Invoke();
        }
    }

    public static int GetSourceId(string sourceGUID)
    {
        if (_modSaveData.SourceGUID_Id.TryGetValue(sourceGUID, out var sourceId))
        {
            return sourceId;
        }

        sourceId = _idPool.AddUnusedItem();
        Log.LogInfo($"Added new source id {sourceId} for previously unregistered GUID {sourceGUID}");
        _modSaveData.SourceGUID_Id.Add(sourceGUID, sourceId);
        _sourceId_GUID.Add(sourceId, sourceGUID);
        return sourceId;
    }

    public static string GetSourceGUID(int sourceId)
    {
        if (_sourceId_GUID.TryGetValue(sourceId, out var guid))
        {
            return guid;
        }

        //warning
        return null;
    }

    public static void SetSourceSave(int sourceId, string data)
    {
        _modSaveData.SourceSaves ??= new Dictionary<int, string>();
        _modSaveData.SourceSaves[sourceId] = data;
    }

    public static string GetSourceSave(int sourceId) => (_modSaveData.SourceSaves?.TryGetValue(sourceId, out var saveString) ?? false)
        ? saveString :
        null;

    public static bool TryExecute(string commandName, string[] parameters, out string result)
    {
        if (_commands.TryGetValue(commandName, out var command))
        {
            try
            {
                return command.Invoke(parameters, out result);
            }
            catch (Exception e)
            {
                Log.LogError("Command exception", e);
                result = "Exception thrown while executing command";
                return false;
            }
        }

        result = $"No command {commandName} found";
        return false;
    }

    public static void AddCommand(ICommand command)
    {
        if (command == null || command.Name == null)
        {
            Log.LogError($"Attempt to register invalid command {command}");
            return;
        }

        _commands[command.Name.ToUpper()] = command;
    }

    public static void AddDataMod(AbilityDataMod mod)
    {
        if (mod == null) { return; }
        _abilityDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Ability, mod.Id);
    }

    public static void AddDataMod(AilmentDataMod mod)
    {
        if (mod == null) { return; }
        _ailmentDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Ailment, mod.Id);
    }

    public static void AddDataMod(CodeDataMod mod)
    {
        if (mod == null) { return; }
        _codeDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Code, mod.Id);
    }

    public static void AddDataMod(CutsceneDataMod mod)
    {
        if (mod == null) { return; }
        _cutsceneDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Cutscene, mod.Id);
    }

    public static void AddDataMod(DialogTriggerDataMod mod)
    {
        if (mod == null) { return; }
        _dialogTriggerDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.DialogTrigger, mod.Id);
    }

    public static void AddDataMod(DlcDataMod mod)
    {
        if (mod == null) { return; }
        _dlcDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Dlc, mod.Id);
    }

    public static void AddDataMod(EnergyDataMod mod)
    {
        if (mod == null) { return; }
        _energyDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Energy, mod.Id);
    }

    public static void AddDataMod(GirlDataMod mod)
    {
        if (mod == null) { return; }
        _girlDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Girl, mod.Id);
    }
    public static void AddDataMod(GirlPairDataMod mod)
    {
        if (mod == null) { return; }
        _girlPairDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.GirlPair, mod.Id);
    }
    public static void AddDataMod(ItemDataMod mod)
    {
        if (mod == null) { return; }
        _itemDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Item, mod.Id);
    }
    public static void AddDataMod(LocationDataMod mod)
    {
        if (mod == null) { return; }
        _locationDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Location, mod.Id);
    }
    public static void AddDataMod(PhotoDataMod mod)
    {
        if (mod == null) { return; }
        _photoDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Photo, mod.Id);
    }
    public static void AddDataMod(QuestionDataMod mod)
    {
        if (mod == null) { return; }
        _questionDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Question, mod.Id);
    }
    public static void AddDataMod(TokenDataMod mod)
    {
        if (mod == null) { return; }
        _tokenDataMods.Add(mod);
        _data.TryRegisterData(GameDataType.Token, mod.Id);
    }
}