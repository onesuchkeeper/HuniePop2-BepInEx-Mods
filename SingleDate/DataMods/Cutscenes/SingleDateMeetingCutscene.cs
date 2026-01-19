using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

public static class SingleDateMeetingCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.Meeting, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggers.GreetingAfternoon, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
            }
        });
    }
}
