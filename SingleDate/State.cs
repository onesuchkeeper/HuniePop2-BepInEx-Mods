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

    public static SaveFile Save => _save;
    private static SaveFile _save;

    public static bool IsSingleDate => _isSingleDate;
    private static bool _isSingleDate;

    public static bool IsSingle(GirlPairDefinition def)
    {
        if (def == null)
        {
            return false;
        }

        var id = ModInterface.Data.GetDataId(GameDataType.GirlPair, def.id);

        return id.SourceId == ModId;
    }

    public static float GetBrokenMult(RelativeId girlId)
    {
        if (!Save.SensitivityLevel.TryGetValue(girlId, out var sensitivityLevel))
        {
            sensitivityLevel = 0;
            Save.SensitivityLevel[girlId] = sensitivityLevel;
        }

        return _baseBrokenMult - (sensitivityLevel * _deltaBrokenMult);
    }

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
            _save = JsonConvert.DeserializeObject<SaveFile>(saveStr);
        }

        _save ??= new SaveFile();
        _save.Clean();
    }
}
