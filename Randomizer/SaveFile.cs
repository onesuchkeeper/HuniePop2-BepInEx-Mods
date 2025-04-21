using System;

namespace Hp2Randomizer;

[Serializable]
public class SaveFile
{
    public int Seed;
    public bool RandomizeNames = true;
    public bool RandomizeBaggage = true;
    public bool RandomizePairs = true;
    public bool RandomizeAffection = true;
    public bool IncludeKyu = true;
    public bool IncludeNymphojinn = true;
    public bool ForceSwapSpecialWithNormal = true;
    public bool SwappedSpecialCharactersKeepWings = true;
    public bool Disable = false;
}