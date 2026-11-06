using System;
using Hp2BaseMod;
using Newtonsoft.Json;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Shared state and helpers for single dates: whether the current date is a single date, the mod's save data, and
/// sensitivity (a save wide track that lowers the affection lost to broken hearts on single dates).
/// </summary>
public static class State
{
    /// <summary>The amount of sensitivity EXP needed for each sensitivity level.</summary>
    public const int ExpPerLevel = 6;

    // Fraction of her current affection a broken heart token removes on a single date at sensitivity level zero.
    private static readonly float _baseBrokenMult = 0.12f;

    // How much that fraction is reduced by at max sensitivity level. It's the same however high the max level is set.
    private static readonly float _maxBrokenMultReduction = 0.04f;

    /// <summary>The puzzle grid's anchored position before any single date shifting, captured when the grid starts.</summary>
    // public static Vector2 DefaultPuzzleGridPosition => _defaultPuzzleGridPosition;
    // private static Vector2 _defaultPuzzleGridPosition;

    /// <summary>The source id of this mod, set on plugin awake.</summary>
    public static int ModId => _modId;
    private static int _modId;

    /// <summary>
    /// The mod's save data for the currently loaded save file slot. Slots that have no data yet are created on demand.
    /// </summary>
    public static SingleSaveFile SaveFile
    {
        get
        {
            // The mod's list is indexed by the base game's save slot, so fill in any slots up to the loaded one.
            while (_save.SaveFiles.Count - 1 < Game.Persistence.loadedFileIndex)
            {
                var saveFile = new SingleSaveFile();
                saveFile.Clean();
                _save.SaveFiles.Add(saveFile);
            }

            return _save.SaveFiles[Game.Persistence.loadedFileIndex];
        }
    }
    private static SingleSaveData _save;

    /// <summary>True if the current location's girl pair is a single date pair.</summary>
    public static bool IsSingleDate => IsSingle(Game.Session?.Location?.currentGirlPair);

    /// <summary>The sensitivity EXP of the loaded save file.</summary>
    public static int SensitivityExp => SaveFile.SensitivityExp;
    /// <summary>Progress to max sensitivity, from 0 to 1.</summary>
    public static float SensitivityPercentage => Mathf.Clamp01(SaveFile.SensitivityExp / (float)MaxSensitivityExp);

    /// <summary>The sensitivity EXP needed to reach max sensitivity level.</summary>
    public static int MaxSensitivityExp => Plugin.MaxSensitivityLevel.Value * ExpPerLevel;

    /// <summary>
    /// True if the pair is a single date pair, which are pairs with Nobody as girl one.
    /// </summary>
    /// <param name="def">The pair to check, can be null.</param>
    public static bool IsSingle(GirlPairDefinition def)
    {
        if (def?.girlDefinitionOne == null)
        {
            return false;
        }

        return ModInterface.Data.GetDataId(GameDataType.Girl, def.girlDefinitionOne.id) == Girls.Nobody;
    }

    /// <summary>
    /// The fraction of the girl's current affection removed by each broken heart match on a single date.
    /// Starts at 12% and goes down linearly to 8% at max sensitivity level.
    /// </summary>
    public static float GetBrokenMult()
    {
        var maxLevel = Plugin.MaxSensitivityLevel.Value;
        var progress = maxLevel > 0 ? Mathf.Clamp01(GetSensitivityLevel() / (float)maxLevel) : 1f;

        return _baseBrokenMult - (_maxBrokenMultReduction * progress);
    }

    /// <summary>The sensitivity level, from 0 to <see cref="Plugin.MaxSensitivityLevel"/>. (The UI displays it plus one.)</summary>
    public static int GetSensitivityLevel() => Math.Min(SaveFile.SensitivityExp / ExpPerLevel, Plugin.MaxSensitivityLevel.Value);

    // /// <summary>Remembers where the puzzle grid is positioned by default. NOTE: nothing currently calls this.</summary>
    // public static void On_UiPuzzleGrid_Start(UiPuzzleGrid uiPuzzleGrid)
    // {
    //     _defaultPuzzleGridPosition = uiPuzzleGrid.GetComponent<RectTransform>().anchoredPosition;
    // }

    /// <summary>
    /// On arriving at a location, blocks drops on the empty left doll and moves the cellphone to the left on single
    /// dates, the opposite for double dates. NOTE: nothing currently calls this, the same is done in
    /// <see cref="ModEventHandles"/> via the arrival events.
    /// </summary>
    // public static void On_LocationManger_Arrive(GirlPairDefinition pair)
    // {
    //     ModInterface.Log.Message("State isSingleDate init");
    //     var isSingleDate = IsSingle(pair);
    //     Game.Session.gameCanvas.dollLeft.dropZoneCanvasGroup.blocksRaycasts = !isSingleDate;
    //     ModInterface.State.CellphoneOnLeft = isSingleDate;
    // }

    /// <summary>Looks up this mod's source id, must run before any ids are built from <see cref="ModId"/>.</summary>
    public static void On_Plugin_Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
    }

    /// <summary>Writes the mod's save data into the game's save as JSON under this mod's source id.</summary>
    public static void On_PreGameSave()
    {
        ModInterface.SetSourceSave(State.ModId, JsonConvert.SerializeObject(_save));
    }

    /// <summary>Reads the mod's save data when a save is loaded, starting empty if there is none, and drops data for content that is no longer registered.</summary>
    public static void On_PostPersistenceReset(SaveData data)
    {
        var saveStr = ModInterface.GetSourceSave(State.ModId);

        _save = string.IsNullOrWhiteSpace(saveStr)
            ? new SingleSaveData()
            : JsonConvert.DeserializeObject<SingleSaveData>(saveStr) ?? new SingleSaveData();
        _save.Clean();
    }

    /// <summary>Clears the mod's data for a save slot when the game resets that slot.</summary>
    internal static void On_SaveFileReset(int fileIndex)
    {
        if (_save?.SaveFiles != null 
            && fileIndex >= 0 
            && fileIndex < _save.SaveFiles.Count)
        {
            var cleanFile = new SingleSaveFile();
            cleanFile.Clean();
            _save.SaveFiles[fileIndex] = cleanFile;
        }
    }
}
