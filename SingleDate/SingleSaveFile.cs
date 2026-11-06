using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace SingleDate;

[Serializable]
/// <summary>The mod's data for one save slot.</summary>
public class SingleSaveFile
{
    /// <summary>Sensitivity EXP earned from smoothies, shared by all girls. See <see cref="State.GetSensitivityLevel"/>.</summary>
    public int SensitivityExp;
    /// <summary>Single date progress for each girl, by girl id.</summary>
    public Dictionary<RelativeId, SingleSaveGirl> Girls;

    /// <summary>Creates missing data and removes girls that are no longer registered (for example, a removed mod's girls).</summary>
    public void Clean()
    {
        Girls ??= new();

        Girls = Girls.Where(x => ModInterface.Data.IsRegistered(GameDataType.Girl, x.Key)).ToDictionary(x => x.Key, x => x.Value);
        Girls.Values.ForEach(x => x.Clean());
    }

    /// <summary>Gets a girl's progress by her runtime (in game) id.</summary>
    public SingleSaveGirl GetGirl(int runtimeId)
        => GetGirl(ModInterface.Data.GetDataId(GameDataType.Girl, runtimeId));

    /// <summary>
    /// Gets a girl's progress, creating it if she has none yet. Returns null for special characters like Nobody.
    /// </summary>
    public SingleSaveGirl GetGirl(RelativeId girlId)
    {
        if (ModInterface.GameData.GetGirl(girlId).specialCharacter)
        {
            return null;
        }

        return Girls.GetOrNew(girlId);
    }
}
