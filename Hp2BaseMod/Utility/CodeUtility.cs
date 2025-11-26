namespace Hp2BaseMod.Utility;

public static class CodeUtility
{
    /// <summary>
    /// In the given data sets the code to either locked or unlocked.
    /// </summary>
    /// <param name="data"> Data holding locked/unlocked state. </param>
    /// <param name="codeId"> Id of the code. </param>
    /// <param name="isUnlocked">If the code should be unlocked.</param>
    public static void ValidateCode(SaveData data, RelativeId codeId, bool isUnlocked)
    {
        var runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Code, codeId);
        if (isUnlocked)
        {
            if (!data.unlockedCodes.Contains(runtimeId))
            {
                data.unlockedCodes.Add(runtimeId);
            }
        }
        else
        {
            data.unlockedCodes.Remove(runtimeId);
        }
    }
}