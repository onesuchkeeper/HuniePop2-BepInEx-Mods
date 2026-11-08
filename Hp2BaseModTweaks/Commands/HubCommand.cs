namespace Hp2BaseMod;

public class HubCommand : ICommand
{
    public string Name => "Hub";

    public string Help => "Accepts 1 argument for save file index. Changes location and state to the hub.";

    public bool Invoke(string[] inputs, out string result)
    {
        if (inputs.Length != 1)
        {
            result = "Expected 1 argument for save file index.";
            return false;
        }

        if (!int.TryParse(inputs[0], out var index))
        {
            result = $"Failed to parse {inputs[0]} as an index.";
            return false;
        }

        if (index < 0 || index >= Game.Persistence.playerData.files.Count)
        {
            result = $"Index {index} out fo range.";
            return false;
        }

        var file = Game.Persistence.playerData.files[index];
        file.locationDefinition = ModInterface.GameData.GetLocation(LocationId.HotelRoom);
        file.girlPairDefinition = null;
        ModInterface.Save.ModFiles[index].GameStatePayload = new GameStateHub.Payload();

        result = $"File {index} set the the hub.";
        return true;
    }
}
