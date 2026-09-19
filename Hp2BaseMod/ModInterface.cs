using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hp2BaseMod.Elements;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Save;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Main interface for interacting with the Hp2BaseMod
/// </summary>
public static class ModInterface
{
    private static string _modSavePath = Path.Combine(Application.persistentDataPath, @"ModSaveData.json");

    /// <summary>
    /// Commands registered to the mod interface
    /// </summary>
    public static IReadOnlyDictionary<string, ICommand> Commands => _commands;
    private static Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

    private static Dictionary<int, Dictionary<string, object>> _interModValues = new Dictionary<int, Dictionary<string, object>>();

    /// <summary>
    /// The session's log
    /// </summary>
    public static ModLog Log => _log;
    private static ModLog _log;

    /// <summary>
    /// Meta information about modded data.
    /// </summary>
    public static ModData Data => _data;
    private static ModData _data;

    /// <summary>
    /// State variables exposed to allow for tweaks.
    /// </summary>
    public static ModState State => _state;
    private static ModState _state = new ModState();

    /// <summary>
    /// Events raised by the base mod for dependant mods.
    /// </summary>
    public static ModEvents Events => _events;
    private static ModEvents _events = new ModEvents();

    /// <summary>
    /// Holds references to requested in-game assets.
    /// </summary>
    public static AssetProvider Assets => _assetProvider;
    private static AssetProvider _assetProvider = new AssetProvider();

    /// <summary>
    /// Provides access to GameData by RelativeId, Do not access Game.Data or this
    /// until Events.PostDataMods triggers
    /// </summary>
    public static GameDefinitionProvider GameData => _gameData;
    private static GameDefinitionProvider _gameData;

    public static DataModRegistry DataMod => _dataMod;
    private static DataModRegistry _dataMod;

    internal static ModSaveData Save => _modSaveData;
    private static ModSaveData _modSaveData;

    public static GameStateManager GameState => _gameState;
    private static GameStateManager _gameState;

    private static RangeSet<int> _idPool;
    private static Dictionary<int, string> _sourceId_GUID;

    private static JsonSerializerSettings _jsonSettings;

    internal static void Init()
    {
        _gameState = new();
        _dataMod = new DataModRegistry();

        _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        _jsonSettings.Converters.Add(new OptionalValueJsonConverter());

        _log = new ModLog("Hp2BaseMod");

        _data = new ModData();

        //load in id cache
        if (File.Exists(_modSavePath))
        {
            _modSaveData = JsonConvert.DeserializeObject<ModSaveData>(File.ReadAllText(_modSavePath));
            if (_modSaveData == null)
            {
                Log.Warning("Failed to load mod save data");
                _modSaveData = new ModSaveData();
            }
        }
        else
        {
            _modSaveData = new ModSaveData();
        }
        _modSaveData.SourceGUID_Id["HP2"] = -1;

        _idPool = new(new IntOperator(), _modSaveData.SourceGUID_Id.Values);

        _sourceId_GUID = new Dictionary<int, string>();
        foreach (var guid_id in _modSaveData.SourceGUID_Id)
        {
            _sourceId_GUID[guid_id.Value] = guid_id.Key;
        }
    }
    
    internal static void ApplyDataMods()
    {
        if (_dataMod.TryApplyDataMods(out var gameDefinitionProvider))
        {
            _gameData = gameDefinitionProvider;
            _gameState.Init(_gameData._gameStates);
            //TODO, add loading state that transitions into title
            _gameState.ChangeState(GameStateId.Title);
            ModInterface.Events.NotifyPostDataMods();
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
        Log.Message($"Mod data saved");
    }

    internal static void InjectSave(SaveData saveData) => _modSaveData.SetData(saveData);

    /// <summary>
    /// Gets the internal id for the provided sourceGUID. 
    /// sourceGUIDs are not case-sensitive.
    /// If an id for the provided sourceGUID does not exist one will be assigned.
    /// </summary>
    public static int GetSourceId(string sourceGUID)
    {
        sourceGUID = sourceGUID.ToUpper();

        if (_modSaveData.SourceGUID_Id.TryGetValue(sourceGUID, out var sourceId))
        {
            return sourceId;
        }

        sourceId = _idPool.AddUnused();
        Log.Message($"Added new source id {sourceId} for previously unregistered GUID {sourceGUID}");
        _modSaveData.SourceGUID_Id.Add(sourceGUID, sourceId);
        _sourceId_GUID.Add(sourceId, sourceGUID);
        return sourceId;
    }

    /// <summary>
    /// Attempts to get the internal id for the provided sourceGUID. Returns false if no internal id has been assigned. 
    /// </summary>
    public static bool TryGetSourceId(string sourceGUID, out int id)
    {
        if (sourceGUID == null)
        {
            id = default;
            return false;
        }

        sourceGUID = sourceGUID.ToUpper();

        if (sourceGUID == "HP2")
        {
            id = -1;
            return true;
        }
        else if (_modSaveData.SourceGUID_Id.TryGetValue(sourceGUID, out var sourceId))
        {
            id = sourceId;
            return true;
        }

        id = default;
        return false;
    }

    /// <summary>
    /// Returns the sourceGUID for the internal id. Returns null if no internal id has been assigned.
    /// </summary>
    public static string GetSourceGUID(int sourceId)
    {
        if (sourceId == -1)
        {
            return "HP2";
        }
        else if (_sourceId_GUID.TryGetValue(sourceId, out var guid))
        {
            return guid;
        }

        throw new Exception($"SourceId {sourceId} has not been registered");
    }

    /// <summary>
    /// Sets the source's save string. This string will be added to the save file and available on load.
    /// </summary>
    public static void SetSourceSave(int sourceId, string data)
    {
        _modSaveData.SourceSaves ??= new Dictionary<int, string>();
        _modSaveData.SourceSaves[sourceId] = data;
    }

    /// <summary>
    /// Returns the source's save string or null if one has not been set
    /// </summary>
    public static string GetSourceSave(int sourceId) => (_modSaveData.SourceSaves?.TryGetValue(sourceId, out var saveString) ?? false)
        ? saveString :
        null;

    /// <summary>
    /// Attempts to execute the <see cref="ICommand"/> with the given name using the given parameters.
    /// </summary>
    public static bool TryExecuteCommand(string commandName, string[] parameters, out string result)
    {
        if (_commands.TryGetValue(commandName, out var command))
        {
            try
            {
                return command.Invoke(parameters, out result);
            }
            catch (Exception e)
            {
                Log.Error("Command exception", e);
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
            Log.Error($"Attempt to register invalid command {command}");
            return;
        }

        if (command.Name.Any(x => x == '.' || x == ' ' || x == '\t'))
        {
            Log.Error($"Invalid command name \"{command.Name}\"");
            return;
        }

        _commands[command.Name.ToUpper()] = command;
    }

    /// <summary>
    /// InterMod Values are accessible to all mods
    /// </summary>
    public static void RegisterInterModValue(int modId, string name, object value)
    {
        ModInterface.Log.Message($"Registering interop value with name {name}, for mod with id {modId}.");
        ModInterface.Log.IsNull(_interModValues, nameof(_interModValues));
        _interModValues.GetOrNew(modId)[name] = value;
    }

    /// <summary>
    /// Attempts to get a value registered by a mod via <see cref="RegisterInterModValue"/>.<para/>
    /// You may want to add a soft dependency to your mod to make sure the value gets registered first: [BepInDependency("modGuid", BepInDependency.DependencyFlags.SoftDependency)]
    /// </summary>
    /// <returns>True if value exists, false otherwise</returns>
    public static bool TryGetInterModValue<T>(string modGuid, string name, out T value) => TryGetInterModValue<T>(GetSourceId(modGuid), name, out value);

    /// <summary>
    /// Attempts to get a value registered by a mod via <see cref="RegisterInterModValue"/>.<para/>
    /// You may want to add a soft dependency to your mod to make sure the value gets registered first: [BepInDependency("modGuid", BepInDependency.DependencyFlags.SoftDependency)]
    /// </summary>
    /// <returns>True if value exists, false otherwise</returns>
    public static bool TryGetInterModValue<T>(int modId, string name, out T value)
    {
        if (_interModValues.TryGetValue(modId, out var modValues)
            && modValues.TryGetValue(name, out var valueObj)
            && valueObj is T asT)
        {
            value = asT;
            return true;
        }

        value = default;
        return false;
    }

    public static IEnumerable<PhotoDefinition> RequestPlayerPhotos()
    {
        var earnedPhotos = Game.Persistence.playerFile.girlPairs
                        .Where(x => x.relationshipType == GirlPairRelationshipType.LOVERS)
                        .Select(x => x.girlPairDefinition.photoDefinition)
                        .Append(Game.Session.Hub.tutorialPhotoDef);

        if (Game.Persistence.playerFile.IsProgressPoolsideEnding())
        {
            earnedPhotos = earnedPhotos.Concat(Game.Session.Hub.nymphojinnPhotoDefs);
        }

        if (Game.Persistence.playerFile.IsProgressPostGame())
        {
            var kyuHole = Game.Persistence.playerFile.GetFlagValue(Flags.KYU_HOLE_SELECTION);
            earnedPhotos = earnedPhotos.Append(Game.Session.Hub.kyuPhotoDefs[Mathf.Clamp(kyuHole, 0, Game.Session.Hub.kyuPhotoDefs.Length - 1)]);
        }

        var args = new RequestUnlockedPhotosEventArgs() { 
            UnlockedPhotos = earnedPhotos.ToList() 
        };
        _events.NotifyRequestUnlockedPhotos(args);

        return args.UnlockedPhotos.Distinct().OrderBy(x => x.id);
    }
}
