namespace Hp2BaseMod.Commands;

public class ArtCommand : ICommand
{
    public string Name => "art";

    public string Help => "Prints some ASCII art of Hp1 girls to the log";

    public bool Invoke(string[] inputs, out string result)
    {
        ModInterface.Log.LogInfo(Art.Random());
        result = "Art printed to log";
        return true;
    }
}