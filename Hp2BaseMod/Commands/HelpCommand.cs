// Hp2BaseMod 2025, By OneSuchKeeper

namespace Hp2BaseMod.Commands;

/// <summary>
/// Accepts the name of a command. Displays information about the command
/// </summary>
public class HelpCommand : ICommand
{
    ///<inheritdoc/>
    public string Name => "help";

    ///<inheritdoc/>
    public string Help => "Accepts the name of a command. Displays information about the command";

    ///<inheritdoc/>
    public bool Invoke(string[] inputs, out string result)
    {
        if (inputs.Length != 1)
        {
            result = "Expected 1 input with the name of a command";
            return false;
        }

        if (ModInterface.Commands.TryGetValue(inputs[0], out var command))
        {
            result = command.Help;
            return true;
        }

        result = $"No command \"{inputs[0]}\" found";
        return false;
    }
}