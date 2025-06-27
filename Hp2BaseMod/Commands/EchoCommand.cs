// Hp2BaseMod 2025, By OneSuchKeeper

namespace Hp2BaseMod.Commands;

/// <summary>
/// Repeats all input arguments back to the user
/// </summary>
public class EchoCommand : ICommand
{
    ///<inheritdoc/>
    public string Name => "echo";

    ///<inheritdoc/>
    public string Help => "Repeats all input arguments back to the user";

    ///<inheritdoc/>
    public bool Invoke(string[] inputs, out string result)
    {
        result = string.Join(" ", inputs);
        return true;
    }
}