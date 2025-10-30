using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;
public static class SingleDateAttractCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.Attract, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                //big move dialogue
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionId = new RelativeId(-1, 34)//big move
                },

                //wait 0.5 sec
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.NOTHING,
                    ProceedFloat = 0.5f,
                    ProceedType = CutsceneStepProceedType.WAIT,
                },

                /// show photo, shown photo is hard coded to the pair data
                /// so instead this is handled in <see cref="EventHandles.On_SinglePhotoDisplayed"/>
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.SHOW_WINDOW,
                    BoolValue = false,
                    WindowPrefabName = "PhotosWindow",
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                },
            }
        });
    }
}
