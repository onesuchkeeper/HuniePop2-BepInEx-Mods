using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class PostSexCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Cutscenes.PostSex, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeWaitInfo(0.5f),
                new FunctionalCutsceneStepInfo((complete) => {
                    //the game requires you to be at the sex location to level up normally, so do it here instead
                    Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair).RelationshipLevelUp();
                    Game.Persistence.playerFile.daytimeElapsed += 1;
                    Game.Persistence.playerFile.PopulateFinderSlots();
                    complete.Invoke();
                }),

                CutsceneStepUtility.MakeBannerTextHideInfo(CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeDollMoveInfo( DollPositionType.HIDDEN, CutsceneStepDollTargetType.RANDOM, CutsceneStepProceedType.INSTANT ),
                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),

                new ChangeLocationCutsceneStep.Info(),

                new FunctionalCutsceneStepInfo((complete) => {
                    var doll = Game.Session.gameCanvas.dollRight;
                    doll.ChangeOutfit();

                    // update time in header
                    Game.Session.gameCanvas.header.Refresh();

                    complete.Invoke();
                }),

                CutsceneStepUtility.MakeToggleHeaderInfo(true, CutsceneStepProceedType.INSTANT),
                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER, CutsceneStepDollTargetType.RANDOM, CutsceneStepProceedType.AUTOMATIC),

                CutsceneStepUtility.MakeWaitInfo(0.25f),
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.PostSex, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeWaitInfo(0.25f),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.AddDataMod(mod);
    }
}
