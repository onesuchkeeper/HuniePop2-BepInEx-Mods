using System;
using System.Security.Cryptography;
using System.Text;
using Hp2BaseMod.Commands;

namespace Hp2Randomizer;

public class SetSeedCommand : ICommand
{
    public string Name => "SetSeed";

    public string Help => "Input any string to set the randomizer seed, or leave blank for a random one. The game must be restarted for it to take effect.";

    public bool Invoke(string[] inputs, out string result)
    {
        int seed;

        if (inputs.Length == 0)
        {
            seed = Environment.TickCount;
        }
        else
        {
            var str = inputs.Length > 1 ? string.Concat(inputs) : inputs[0];
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            seed = BitConverter.ToInt32(hashed, 0);
        }

        Plugin.Seed.Value = seed;

        result = $"Seed set to {seed}. The game must be saved then restarted for it to take effect.";
        return true;
    }
}