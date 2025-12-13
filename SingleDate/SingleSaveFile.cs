using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace SingleDate;

[Serializable]
public class SingleSaveFile
{
    public int SensitivityExp;
    public Dictionary<RelativeId, SingleSaveGirl> Girls;

    public void Clean()
    {
        Girls ??= new();

        Girls = Girls.Where(x => ModInterface.Data.IsRegistered(GameDataType.Girl, x.Key)).ToDictionary(x => x.Key, x => x.Value);
        Girls.Values.ForEach(x => x.Clean());
    }

    public SingleSaveGirl GetGirl(int runtimeId)
        => GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, runtimeId));

    public SingleSaveGirl GetGirl(RelativeId girlId)
    {
        if (ModInterface.GameData.GetGirl(girlId).specialCharacter)
        {
            return null;
        }

        return Girls.GetOrNew(girlId);
    }
}
