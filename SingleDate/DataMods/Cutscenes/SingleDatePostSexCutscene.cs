using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;
public static class SingleDatePostSexCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.PostSex, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 43)//moan 1
                }
            }
        });
    }
}
