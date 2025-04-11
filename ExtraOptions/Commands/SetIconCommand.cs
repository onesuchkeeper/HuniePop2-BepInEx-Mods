using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Commands;
using Hp2BaseMod.ModLoader;

namespace Hp2ExtraOptions;

public class SetIconCommand : ICommand
{
    private static FieldInfo _saveDataAccess = AccessTools.Field(typeof(GamePersistence), "_saveData");

    public string Name => "seticon";

    public string Help => "Accepts the index of a file and the id of a girl. Sets the icon of the file to the girl.";

    public bool Invoke(string[] inputs, out string result)
    {
        var saveData = (SaveData)_saveDataAccess.GetValue(Game.Persistence);

        if (inputs.Length != 2)
        {
            result = "Expected 2 input parameters";
            return false;
        }

        if (!(int.TryParse(inputs[0], out var fileIndex) && fileIndex >= 0 && fileIndex < saveData.files.Count))
        {
            result = $"Unable to parse {inputs[0]} as an int in range [0-{saveData.files.Count - 1}]";
            return false;
        }

        if (!(int.TryParse(inputs[1], out var girlId) && girlId > 0 && girlId < 14))
        {
            result = $"Unable to parse {inputs[1]} as a girl id (1-13)";
            return false;
        }

        saveData.files[fileIndex].fileIconGirlId = girlId;

        result = $"Icon of file {fileIndex} set to girl {girlId}. Title menu will need to be reloaded to see change.";
        return true;
    }
}