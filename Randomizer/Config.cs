using System;

namespace Hp2Randomizer;

[Serializable]
public class Config
{
    public int Seed;
    public bool RandomizeNames = true;
    public bool RandomizeBaggage = true;
    public bool RandomizePairs = true;
    public bool RandomizeAffection = true;
    public bool IncludeSpecialCharacters = true;
    public bool ForceSpecialCharacters = true;
    public bool SwappedSpecialCharactersKeepWings = true;
    public bool Disable = false;
}