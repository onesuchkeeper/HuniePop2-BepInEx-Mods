using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class PreSexCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Cutscenes.PreSex, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PreBedroom, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),

                CutsceneStepUtility.MakeRandomDollMoveInfo(DollPositionType.HIDDEN, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),

                new ChangeLocationCutsceneStep.Info(Plugin.SexLocs.ToArray()),
                
                // change outfit
                new FunctionalCutsceneStepInfo((complete) => {
                    var doll = Game.Session.gameCanvas.dollRight;
                    doll.ChangeOutfit(doll.girlDefinition.Expansion().GetOutfitIndex(Hp2BaseMod.Styles.Sexy));

                    complete.Invoke();
                }),

                CutsceneStepUtility.MakeRandomDollMoveInfo(DollPositionType.INNER, CutsceneStepProceedType.AUTOMATIC),

                CutsceneStepUtility.MakeWaitInfo(0.5f),

                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PreSex, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),

                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeShowPuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.AddDataMod(mod);
    }
}
