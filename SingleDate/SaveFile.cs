using System.Collections.Generic;
using Hp2BaseMod;

namespace SingleDate;

public class SaveFile
{
    public Dictionary<RelativeId, int> SensitivityLevel;

    public bool ShowUpsetHint;

    public void Clean()
    {
        SensitivityLevel ??= new Dictionary<RelativeId, int>();
    }
}