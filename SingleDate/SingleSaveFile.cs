using System;
using System.Collections.Generic;
using Hp2BaseMod;

namespace SingleDate;

[Serializable]
public class SingleSaveFile
{
    public Dictionary<RelativeId, SingleSaveGirl> Girls;

    public void Clean()
    {
        Girls ??= new Dictionary<RelativeId, SingleSaveGirl>();
    }

    public SingleSaveGirl GetGirl(int runtimeId)
        => GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, runtimeId));

    public SingleSaveGirl GetGirl(RelativeId girlId)
    {
        if (ModInterface.GameData.GetGirl(girlId).specialCharacter)
        {
            return null;
        }

        if (!Girls.TryGetValue(girlId, out var singleSaveGirl))
        {
            singleSaveGirl = new SingleSaveGirl();
            Girls[girlId] = singleSaveGirl;
        }

        return singleSaveGirl;
    }
}
