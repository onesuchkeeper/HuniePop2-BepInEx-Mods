using BepInEx;
using Hp2BaseMod;
using Newtonsoft.Json;
using UnityEngine;

namespace SingleDate;

public static class State
{
    private static readonly float _baseBrokenMult = 0.12f;
    private static readonly float _deltaBrokenMult = 0.01f;

    public static Vector3 DefaultPuzzleGridPosition => _defaultPuzzleGridPosition;
    private static Vector3 _defaultPuzzleGridPosition;

    public static int ModId => _modId;
    private static int _modId;

    public static SaveFile Save
    {
        get
        {
            while (_save.SaveFiles.Count - 1 < Game.Persistence.loadedFileIndex)
            {
                _save.SaveFiles.Add(new SaveFile());
            }

            return _save.SaveFiles[Game.Persistence.loadedFileIndex];
        }
    }
    private static SaveData _save;

    public static bool ShowSingleUpsetHint => _save.ShowSingleUpsetHint;

    public static bool IsSingleDate => _isSingleDate;
    private static bool _isSingleDate;

    public static bool IsSingle(GirlPairDefinition def)
    {
        if (def == null || def.girlDefinitionOne == null)
        {
            return false;
        }

        return ModInterface.Data.GetDataId(GameDataType.Girl, def.girlDefinitionOne.id) == GirlNobody.Id;
    }

    public static float GetBrokenMult() => _baseBrokenMult - (GetSensitivityLevel() * _deltaBrokenMult);

    public static int GetSensitivityLevel() => Save.SensitivityExp / 6;

    public static void On_UiPuzzleGrid_Start(UiPuzzleGrid uiPuzzleGrid)
    {
        _defaultPuzzleGridPosition = uiPuzzleGrid.transform.position;
    }

    public static void On_LocationManger_Arrive()
    {
        _isSingleDate = IsSingle(Game.Persistence.playerFile.girlPairDefinition);
    }

    public static void On_Plugin_Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
    }

    public static void On_PreGameSave()
    {
        ModInterface.SetSourceSave(State.ModId, JsonConvert.SerializeObject(State.Save));
    }

    public static void On_PostPersistenceReset()
    {
        var saveStr = ModInterface.GetSourceSave(State.ModId);

        if (!saveStr.IsNullOrWhiteSpace())
        {
            _save = JsonConvert.DeserializeObject<SaveData>(saveStr);
        }

        _save ??= new SaveData();
        _save.Clean();
    }
}
