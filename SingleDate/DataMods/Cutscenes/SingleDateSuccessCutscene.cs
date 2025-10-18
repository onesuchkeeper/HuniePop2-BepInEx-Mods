using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;
public static class SingleDateSuccessCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.Success, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.BANNER_TEXT,
                    BoolValue = true,
                    BannerTextPrefabName = "BannerTextSuccess",//fix this, use the right one...
                    IntValue = 0
                },
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.PUZZLE_REFOCUS,
                    BoolValue = false,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC
                },
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.SPECIAL_STEP,
                    SpecialStepPrefabName = "CutsceneStepPostRewards"
                },
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionID = new RelativeId(-1, 16)//date success
                },
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.BANNER_TEXT,
                    BoolValue = false
                },
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
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.PUZZLE_GRID,
                    BoolValue = false
                },
                new CutsceneStepInfo()
                {
                    StepType = CutsceneStepType.NOTHING,
                    ProceedFloat = 0.5f,
                    ProceedType = CutsceneStepProceedType.WAIT,
                }
            }
        });
    }
}
