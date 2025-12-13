using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

public static class SingleDateAttractNoPhotoCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.AttractNoPhoto, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                // big move dialogue
                CutsceneStepUtility.MakeDialogTriggerInfo(new RelativeId(-1, 34), CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
            }
        });
    }
}
