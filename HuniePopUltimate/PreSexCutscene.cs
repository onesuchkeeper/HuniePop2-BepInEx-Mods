using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class PreSexCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(new RelativeId(Plugin.ModId, 0), InsertStyle.append)
        {
            Steps = new()
            {
                //dt
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PreBedroom, CutsceneStepProceedType.WAIT, CutsceneStepDollTargetType.RANDOM),
                //hide puzzle
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.PUZZLE_GRID,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    BoolValue = false
                },
                //hide cellphone
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.TOGGLE_PHONE,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    IntValue = 0,
                    BoolValue = false,
                },
                //move off screen
                new CutsceneStepInfo() {
                    StepType = CutsceneStepType.DOLL_MOVE,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DollPositionType = DollPositionType.HIDDEN
                },
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.NOTHING,
                    ProceedType = CutsceneStepProceedType.STANDBY,
                    ProceedFloat = 0.5f,
                },
                //change loc
                // new CutsceneStepInfo() {
                //     StepType = CutsceneStepType.GAME_ACTION,
                //     ProceedType = CutsceneStepProceedType.WAIT,
                //     LogicActionInfo = new LogicActionInfo() {
                //         Type = LogicActionType.TRAVEL_TO,
                //         LocationDefinitionID = LocationIds.BedRoomDate
                //     }
                // },
                //change outfit//work on this
                // new CutsceneStepInfo() {
                //     StepType = CutsceneStepType.LOAD_GIRL,
                //     DollTargetType = CutsceneStepDollTargetType.RANDOM,
                //     BoolValue = true,
                //     HairstyleId = ,//hmm
                //     OutfitId = ,//yeah
                //     GirlDefinitionID = Girls.Celeste,//this kinda sucks, ill need a new cutscene for each
                // },
                //show cellphone
                
                //move on screen
                new CutsceneStepInfo() {
                    StepType = CutsceneStepType.DOLL_MOVE,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DollPositionType = DollPositionType.INNER
                },
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.TOGGLE_PHONE,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    IntValue = 0,
                    BoolValue = true,
                },
                //dt
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.DIALOG_TRIGGER,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    DollTargetType = CutsceneStepDollTargetType.RANDOM,
                    DialogTriggerDefinitionId = DialogTriggers.PreSex
                },
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.NOTHING,
                    ProceedType = CutsceneStepProceedType.WAIT,
                    ProceedFloat = 0.5f,
                },
                //show puzzle
                new CutsceneStepInfo(){
                    StepType = CutsceneStepType.PUZZLE_GRID,
                    ProceedType = CutsceneStepProceedType.AUTOMATIC,
                    BoolValue = true
                },
            }
        };

        ModInterface.AddDataMod(mod);
    }
}
