namespace Hp2BaseMod.Commands;

public class EchoCommand : ICommand
{
    public string Name => "echo";

    public string Help => "Repeats all input arguments back to the user";

    public bool Invoke(string[] inputs, out string result)
    {
        result = string.Join(" ", inputs);
        return true;
    }
}