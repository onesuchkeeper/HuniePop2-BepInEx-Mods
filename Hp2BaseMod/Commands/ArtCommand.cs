// Hp2BaseMod 2025, By OneSuchKeeper

namespace Hp2BaseMod.Commands;

/// <summary>
/// Prints some ASCII art of Hp1 girls to the log
/// </summary>
public class ArtCommand : ICommand
{
    ///<inheritdoc/>
    public string Name => "art";

    ///<inheritdoc/>
    public string Help => "Prints some ASCII art of Hp1 girls to the log";

    ///<inheritdoc/>
    public bool Invoke(string[] inputs, out string result)
    {
        ModInterface.Log.Message(Art.Random());
        result = "Art printed to log";
        return true;
    }
}