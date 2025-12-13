// Hp2RepeatThreesomeMod 2021, By onesuchkeeper

using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace RepeatThreesome
{
    public static class ThreesomeHandler
    {
        private static FieldInfo f_newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");

        public static void OnPuzzleRoundOver(PuzzleRoundOverArgs args)
        {
            if (Game.Session.Puzzle.puzzleStatus.statusType == PuzzleStatusType.NORMAL)
            {
                var currentGirlPair = Game.Session.Location.currentGirlPair;

                var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);

                if (playerFileGirlPair != null
                    && Game.Session.Puzzle.puzzleGrid.roundState == PuzzleRoundState.SUCCESS
                    && playerFileGirlPair.relationshipType == GirlPairRelationshipType.LOVERS
                    && (Game.Session.Location.currentLocation == currentGirlPair.sexLocationDefinition
                        || ModInterface.GameData.IsCodeUnlocked(Constants.LocalCodeId)))
                {
                    args.IsSexDate = true;
                    args.IsGameOver = Game.Session.Puzzle.puzzleStatus.bonusRound;
                }
            }

            Game.Session.Cutscenes.CutsceneCompleteEvent += On_CutsceneCompleteEvent;
        }

        private static void On_CutsceneCompleteEvent()
        {
            Game.Session.Cutscenes.CutsceneCompleteEvent -= On_CutsceneCompleteEvent;

            var newRoundCutscene = f_newRoundCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle);

            if (newRoundCutscene == Game.Session.Puzzle.cutsceneNewroundBossBonus)
            {
                Game.Session.Cutscenes.CutsceneStartedEvent += On_BossBonusStart;
            }
            else if (!Game.Session.Puzzle.puzzleStatus.gameOver
                && ModInterface.GameData.IsCodeUnlocked(Constants.NudeCodeId)
                && Game.Session.Puzzle.puzzleStatus.statusType == PuzzleStatusType.NORMAL)
            {
                var silent = Game.Session.Puzzle.puzzleStatus.girlStatusLeft != null
                    && ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl,
                        Game.Session.gameCanvas.dollLeft,
                        false);

                //ignore kyu during tutorial
                if (!Game.Session.Puzzle.puzzleStatus.IsTutorial(false)
                    && Game.Session.Puzzle.puzzleStatus.girlStatusRight != null)
                {
                    ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl,
                        Game.Session.gameCanvas.dollRight,
                        silent);
                }
            }
        }

        private static void On_BossBonusStart()
        {
            Game.Session.Cutscenes.CutsceneStartedEvent -= On_BossBonusStart;

            var silent = ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl,
                Game.Session.gameCanvas.dollLeft,
                false);

            ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl,
                Game.Session.gameCanvas.dollRight,
                silent);
        }

        private static bool ChangeToNudeOutfit(PlayerFileGirl girl, UiDoll doll, bool silentUnlock)
        {
            if (girl == null
                || doll == null
                || doll.girlDefinition == null)
            {
                return false;
            }

            var expansion = ExpandedGirlDefinition.Get(girl.girlDefinition);

            if (!expansion.OutfitIdToIndex.TryGetValue(Constants.NudeOutfitId, out var nudeOutfitIndex))
            {
                ModInterface.Log.Warning($"Failed to find nude outfit for Girl {girl.girlDefinition.girlName}.");
                return false;
            }

            doll.ChangeOutfit(nudeOutfitIndex);

            return StyleUnlockUtility.UnlockStyle(girl, -1, nudeOutfitIndex, silentUnlock);
        }
    }
}
