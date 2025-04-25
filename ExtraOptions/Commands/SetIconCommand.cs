using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Commands;
using Hp2BaseMod.Extension;

namespace Hp2ExtraOptions;

public class SetIconCommand : ICommand
{
    private static FieldInfo _saveDataAccess = AccessTools.Field(typeof(GamePersistence), "_saveData");

    public string Name => "seticon";

    public string Help => "Accepts the index of a file, the GUID of a source, then the local id of a girl from that source as an int. Sets the icon of the file to the girl. \"/SETICON 0 HP2 13\" will set the first profile's icon to Kyu";

    public bool Invoke(string[] inputs, out string result)
    {
        var saveData = _saveDataAccess.GetValue<SaveData>(Game.Persistence);

        if (inputs.Length != 3)
        {
            result = "Expected 3 input parameters.";
            return false;
        }

        if (!(int.TryParse(inputs[0], out var fileIndex) && fileIndex >= 0 && fileIndex < saveData.files.Count))
        {
            result = $"Unable to parse {inputs[0]} as a valid file index. Should be an int in range [0-{saveData.files.Count - 1}].";
            return false;
        }

        if (!ModInterface.TryGetSourceId(inputs[1], out var sourceId))
        {
            result = $"Unable to find sourceGuid {inputs[1]}.";
            return false;
        }

        if (!int.TryParse(inputs[2], out var girlLocalId))
        {
            result = $"Unable to parse {inputs[2]} an an integer.";
            return false;
        }

        if (!ModInterface.Data.TryGetRuntimeDataId(GameDataType.Girl, new RelativeId(sourceId, girlLocalId), out var runtimeId))
        {
            result = $"Unable to find girl {inputs[2]} from source {inputs[1]}.";
            return false;
        }

        saveData.files[fileIndex].fileIconGirlId = runtimeId;

        result = $"Icon of file {fileIndex} set to girl {inputs[2]} from {inputs[1]}. Title menu will need to be reloaded to see the change.";
        return true;
    }
}
