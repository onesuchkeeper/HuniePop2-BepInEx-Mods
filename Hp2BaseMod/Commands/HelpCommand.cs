namespace Hp2BaseMod.Commands;

public class HelpCommand : ICommand
{
    public string Name => "help";

    public string Help => "Accepts the name of a command. Displays information about the command";

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