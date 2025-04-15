// Hp2RepeatThreesomeMod 2021, By onesuchkeeper

using System;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;

namespace RepeatThreesome
{
    public static class ThreesomeHandler
    {
        private static FieldInfo _roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
        private static FieldInfo _newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");
        private static FieldInfo _gameOver = AccessTools.Field(typeof(PuzzleStatus), "_gameOver");

        public static void PreRoundOverCutscene()
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
                    if (Game.Session.Puzzle.puzzleStatus.bonusRound)
                    {
                        _roundOverCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneSuccessBonus);
                    }
                    else
                    {
                        ModInterface.Log.LogInfo("Setting up Lovers Bonus Round");
                        _roundOverCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneSuccessAttracted);
                        _newRoundCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneNewroundBonus);

                        //gameOver property has been made to only be settable to true, for some reason...
                        //so access the underlying field
                        _gameOver.SetValue(Game.Session.Puzzle.puzzleStatus, false);
                    }
                }
            }

            Game.Session.Cutscenes.CutsceneCompleteEvent += On_CutsceneCompleteEvent;
        }

        private static void On_CutsceneCompleteEvent()
        {
            Game.Session.Cutscenes.CutsceneCompleteEvent -= On_CutsceneCompleteEvent;

            var newRoundCutscene = _newRoundCutscene.GetValue(Game.Session.Puzzle) as CutsceneDefinition;

            if (!Game.Session.Puzzle.puzzleStatus.gameOver
                && ModInterface.GameData.IsCodeUnlocked(Constants.NudeCodeId)
                && (newRoundCutscene == Game.Session.Puzzle.cutsceneNewroundBonus
                    || newRoundCutscene == Game.Session.Puzzle.cutsceneNewroundBossBonus))
            {
                var silent = ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl,
                                    Game.Session.gameCanvas.dollLeft,
                                    Game.Session.gameCanvas.dollLeft.notificationBox,
                                    false);

                if (Game.Session.Puzzle.puzzleStatus.IsTutorial(false))
                {
                    ModInterface.Log.LogInfo("Ignoring nude outfit change for Kyu in tutorial");
                }
                else
                {
                    ChangeToNudeOutfit(Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl,
                        Game.Session.gameCanvas.dollRight,
                        Game.Session.gameCanvas.dollRight.notificationBox,
                        silent);
                }
            }
        }

        private static bool ChangeToNudeOutfit(PlayerFileGirl girl, UiDoll doll, NotificationBoxBehavior notificationBox, bool silent)
        {
            if (girl == null || doll == null) { return false; }

            var outfitIndex = ModInterface.Data.GetOutfitIndex(ModInterface.Data.GetDataId(GameDataType.Girl, girl.girlDefinition.id), Constants.NudeOutfitId);
            doll.ChangeOutfit(outfitIndex);

            if (girl.UnlockOutfit(outfitIndex))
            {
                notificationBox.Show("\"Nude\" Outfit Unlocked!", 4f, silent);
                return true;
            }

            return false;
        }
    }
}
