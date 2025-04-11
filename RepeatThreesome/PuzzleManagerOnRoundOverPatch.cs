﻿// Hp2RepeatThreesomeMod 2021, By onesuchkeeper

using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.ModLoader;
using System;
using System.Reflection;

namespace RepeatThreesome
{
    [HarmonyPatch(typeof(PuzzleManager), "OnRoundOver")]
    class PuzzleManagerOnRoundOverPatch
    {
        public static bool Prefix(PuzzleManager __instance)
        {
            var puzzleGridValue = AccessTools.Field(typeof(PuzzleManager), "_puzzleGrid").GetValue(__instance) as UiPuzzleGrid;
            var puzzleStatusValue = AccessTools.Field(typeof(PuzzleManager), "_puzzleStatus").GetValue(__instance) as PuzzleStatus;

            var roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
            var newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");


            roundOverCutscene.SetValue(__instance, __instance.cutsceneFailure);
            newRoundCutscene.SetValue(__instance, null);
            bool flag = true;
            switch (puzzleStatusValue.statusType)
            {
                case PuzzleStatusType.NORMAL:
                    var currentGirlPair = Game.Session.Location.currentGirlPair;
                    var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);
                    if (playerFileGirlPair != null)
                    {
                        // I'm keeping these two here, but why Dev?
                        Game.Persistence.playerFile.GetPlayerFileGirl(currentGirlPair.girlDefinitionOne);
                        Game.Persistence.playerFile.GetPlayerFileGirl(currentGirlPair.girlDefinitionTwo);

                        if (puzzleGridValue.roundState == PuzzleRoundState.SUCCESS)
                        {
                            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.COMPATIBLE)
                            {
                                roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessCompatible);
                                playerFileGirlPair.RelationshipLevelUp();
                            }
                            else if
                            (
                                (
                                    (
                                        (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED)
                                        ||
                                        (playerFileGirlPair.relationshipType == GirlPairRelationshipType.LOVERS)
                                    )
                                    &&
                                    (Game.Session.Location.currentLocation == currentGirlPair.sexLocationDefinition)
                                )
                                ||
                                (
                                    (playerFileGirlPair.relationshipType == GirlPairRelationshipType.LOVERS)
                                    &&
                                    GameDefinitionProvider.IsCodeUnlocked(Constants.LocalCodeId)
                                )
                            )
                            {
                                if (!puzzleStatusValue.bonusRound)
                                {
                                    roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessAttracted);
                                    newRoundCutscene.SetValue(__instance, __instance.cutsceneNewroundBonus);
                                    flag = false;
                                }
                                else
                                {
                                    roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessBonus);
                                    playerFileGirlPair.RelationshipLevelUp();
                                    if (Game.Persistence.playerFile.completedGirlPairs.Count > 0)
                                    {
                                        Game.Manager.Platform.UnlockAchievement("unchaste", true);
                                        if (Game.Persistence.playerFile.daytimeElapsed < 12)
                                        {
                                            Game.Manager.Platform.UnlockAchievement("unchaste_with_haste", true);
                                        }
                                    }
                                    if (Game.Persistence.playerFile.completedGirlPairs.Count == 24)
                                    {
                                        Game.Manager.Platform.UnlockAchievement("debauchery", true);
                                        if (Game.Persistence.playerFile.daytimeElapsed < 68)
                                        {
                                            Game.Manager.Platform.UnlockAchievement("debauchery_without_delay", true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccess);
                            }

                            if (!puzzleStatusValue.bonusRound)
                            {
                                if (puzzleStatusValue.movesRemaining >= 20)
                                {
                                    Game.Manager.Platform.UnlockAchievement("quickie", true);
                                }
                                if (puzzleStatusValue.girlStatusLeft.passion == 100
                                    && puzzleStatusValue.girlStatusLeft.sentiment == 40
                                    && puzzleStatusValue.girlStatusLeft.stamina == 6
                                    && puzzleStatusValue.girlStatusRight.passion == 100
                                    && puzzleStatusValue.girlStatusRight.sentiment == 40
                                    && puzzleStatusValue.girlStatusRight.stamina == 6)
                                {
                                    Game.Manager.Platform.UnlockAchievement("fill_er_up", true);
                                }
                                if (puzzleStatusValue.girlStatusLeft.exhaustedCount + puzzleStatusValue.girlStatusRight.exhaustedCount >= 8)
                                {
                                    Game.Manager.Platform.UnlockAchievement("like_it_rough", true);
                                }
                            }

                            if (flag
                                && Game.Persistence.playerFile.storyProgress >= 14
                                && Game.Persistence.playerFile.GetFlagValue("alpha_mode") <= 0)
                            {
                                Game.Persistence.playerFile.alphaDateCount++;
                                if (Game.Persistence.playerFile.alphaDateCount >= 8)
                                {
                                    Game.Manager.Platform.UnlockAchievement("alpha_apprentice", true);
                                    if (Game.Persistence.playerFile.alphaDateCount >= 24)
                                    {
                                        Game.Manager.Platform.UnlockAchievement("alpha_adept", true);
                                    }
                                }
                            }
                        }
                    }
                    else if (puzzleGridValue.roundState == PuzzleRoundState.SUCCESS)
                    {
                        roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccess);
                    }
                    break;
                case PuzzleStatusType.NONSTOP:
                    if (puzzleGridValue.roundState == PuzzleRoundState.SUCCESS)
                    {
                        roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessNonstop);
                        newRoundCutscene.SetValue(__instance, __instance.cutsceneNewroundNonstop);
                        flag = false;
                    }
                    else
                    {
                        if (puzzleStatusValue.roundIndex > 0)
                        {
                            roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccess);
                        }
                        if (puzzleStatusValue.roundIndex > Game.Persistence.playerFile.nonstopDateCount)
                        {
                            Game.Persistence.playerFile.nonstopDateCount = puzzleStatusValue.roundIndex;
                            if (Game.Persistence.playerFile.nonstopDateCount >= 8)
                            {
                                Game.Manager.Platform.UnlockAchievement("non_stop_novice", true);
                                if (Game.Persistence.playerFile.nonstopDateCount >= 16)
                                {
                                    Game.Manager.Platform.UnlockAchievement("non_stop_ninja", true);
                                }
                            }
                        }
                    }
                    break;
                case PuzzleStatusType.BOSS:
                    if (puzzleGridValue.roundState == PuzzleRoundState.SUCCESS)
                    {
                        if (puzzleStatusValue.bonusRound)
                        {
                            roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessBonus);
                            if (Game.Persistence.playerFile.storyProgress <= 11)
                            {
                                Game.Persistence.playerFile.storyProgress = 12;
                            }
                            if (Game.Persistence.playerFile.storyProgress >= 12)
                            {
                                Game.Manager.Platform.UnlockAchievement("savior", true);
                                if (Game.Persistence.playerFile.settingDifficulty == SettingDifficulty.HARD)
                                {
                                    Game.Manager.Platform.UnlockAchievement("super_savior", true);
                                }
                            }
                        }
                        else if (puzzleStatusValue.girlListCount <= 2)
                        {
                            roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessBoss);
                            newRoundCutscene.SetValue(__instance, __instance.cutsceneNewroundBossBonus);
                            flag = false;
                        }
                        else
                        {
                            roundOverCutscene.SetValue(__instance, __instance.cutsceneSuccessBoss);
                            newRoundCutscene.SetValue(__instance, __instance.cutsceneNewroundBoss);
                            flag = false;
                        }
                    }
                    else if (Game.Persistence.playerFile.storyProgress < 12
                             && Game.Persistence.playerFile.GetFlagValue("nymphojinn_failure") < 0)
                    {
                        Game.Persistence.playerFile.SetFlagValue("nymphojinn_failure", 0);
                    }
                    break;
            }
            puzzleStatusValue.gameOver = flag;
            new PuzzleManagerEvents(__instance).AddOnRoundOverCutsceneComplete();
            Game.Session.Cutscenes.StartCutscene(roundOverCutscene.GetValue(__instance) as CutsceneDefinition, null);

            return false;
        }
    }

    /// <summary>
    /// Replacement events for puzzle manager
    /// </summary>
    public class PuzzleManagerEvents
    {
        private static string nudeUnlock = "\"Nude\" Outfit Unlocked!";
        PuzzleManager _puzzleManager;
        public PuzzleManagerEvents(PuzzleManager puzzleManager)
        {
            _puzzleManager = puzzleManager;
        }

        public void AddOnRoundOverCutsceneComplete()
        {
            Game.Session.Cutscenes.CutsceneCompleteEvent += OnRoundOverCutsceneComplete;
        }

        private static FieldInfo _puzzleManager_newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");
        private static MethodInfo _puzzleManager_OnRoundOverCutsceneComplete = AccessTools.Method(typeof(PuzzleManager), "OnRoundOverCutsceneComplete");

        public void OnRoundOverCutsceneComplete()
        {
            var newRoundCutsceneValue = _puzzleManager_newRoundCutscene.GetValue(_puzzleManager) as CutsceneDefinition;

            Game.Session.Cutscenes.CutsceneCompleteEvent -= OnRoundOverCutsceneComplete;

            _puzzleManager_OnRoundOverCutsceneComplete.Invoke(_puzzleManager, null);

            if (GameDefinitionProvider.IsCodeUnlocked(Constants.NudeCodeId)
                && (newRoundCutsceneValue == _puzzleManager.cutsceneNewroundBonus
                    || newRoundCutsceneValue == _puzzleManager.cutsceneNewroundBossBonus))
            {
                var silent = ChangeToNudeOutfit(_puzzleManager.puzzleStatus.girlStatusLeft.playerFileGirl,
                                    Game.Session.gameCanvas.dollLeft,
                                    Game.Session.gameCanvas.dollLeft.notificationBox,
                                    false);

                if (Game.Session.Puzzle.puzzleStatus.IsTutorial(false))
                {
                    ModInterface.Log.LogInfo("Ignoring nude outfit change for Kyu in tutorial");
                }
                else
                {
                    ChangeToNudeOutfit(_puzzleManager.puzzleStatus.girlStatusRight.playerFileGirl,
                        Game.Session.gameCanvas.dollRight,
                        Game.Session.gameCanvas.dollRight.notificationBox,
                        silent);
                }
            }
        }

        private bool ChangeToNudeOutfit(PlayerFileGirl girl, UiDoll doll, NotificationBoxBehavior notificationBox, bool silent)
        {
            if (girl == null || doll == null) { return false; }

            var outfitIndex = ModInterface.Data.GetOutfitIndex(ModInterface.Data.GetDataId(GameDataType.Girl, girl.girlDefinition.id), Constants.NudeOutfitId);
            doll.ChangeOutfit(outfitIndex);

            if (!girl.IsOutfitUnlocked(outfitIndex))
            {
                notificationBox.Show(nudeUnlock, 4f, silent);
                girl.UnlockOutfit(outfitIndex);
                return true;
            }

            return false;
        }
    }
}
