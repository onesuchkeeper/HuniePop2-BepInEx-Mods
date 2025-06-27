// Hp2BaseMod 2025, By OneSuchKeeper

namespace Hp2BaseMod.Commands;

/// <summary>
/// Defines a command able to be called by the user via the code ui.
/// Commands may be registered using <see cref="ModInterface.AddCommand"/>
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Name used to access the command. Command names are not case sensitive and must not contain whitespace or the '.' character.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Info displayed to the user for the help command
    /// </summary>
    string Help { get; }

    /// <summary>
    /// Accepts input parameters, preforms the command and returns if command was successful and the resulting message
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    bool Invoke(string[] inputs, out string result);
}