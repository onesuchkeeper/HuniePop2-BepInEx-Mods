using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class PreSexCutscene
{
    internal static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Cutscenes.PreSex, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PreBedroom, CutsceneStepProceedType.AUTOMATIC).TargetOrientation(DollOrientationType.RIGHT),

                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, 1f, CutsceneStepProceedType.INSTANT).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),

                new ChangeLocationCutsceneStep.Info(Plugin.SexLocs.ToArray()),
                
                // change outfit
                new FunctionalCutsceneStepInfo((complete) => {
                    var doll = Game.Session.gameCanvas.dollRight;
                    doll.ChangeOutfit(doll.girlDefinition.GetExpansion().GetOutfitIndex(Hp2BaseMod.Styles.Sexy));

                    complete.Invoke();
                }),

                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER).TargetOrientation(DollOrientationType.RIGHT),

                CutsceneStepUtility.MakeWaitInfo(0.5f),

                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PreSex).TargetOrientation(DollOrientationType.RIGHT),

                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeShowPuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.DataMod.AddDataMod(mod);
    }
}
