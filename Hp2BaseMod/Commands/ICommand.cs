namespace Hp2BaseMod.Commands;

public interface ICommand
{
    /// <summary>
    /// Name used to access the command. Command names are not case sensitive
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Info displayed to the user for the help command
    /// </summary>
    string Help { get; }

    /// <summary>
    /// Accepts input parameters and returns the resulting message
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    bool Invoke(string[] inputs, out string result);
}