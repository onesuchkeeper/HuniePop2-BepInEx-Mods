// Hp2BaseMod 2026, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Whether the forfeit button works is the puzzle state's call (<see cref="IPuzzleState.AllowsForfeit"/>).
/// </summary>
[HarmonyPatch(typeof(UiCellphoneAppSettings))]
internal static class UiCellphoneAppSettingsPatch
{
    private static readonly FieldInfo f_playerData = AccessTools.Field(typeof(UiCellphoneAppSettings), "_playerData");
    private static readonly FieldInfo f_origContainerPos = AccessTools.Field(typeof(UiCellphoneAppSettings), "_origContainerPos");

    [HarmonyPatch(nameof(UiCellphoneAppSettings.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiCellphoneAppSettings __instance)
    {
        var playerData = f_playerData.GetValue<PlayerData>(__instance);
        var playerFile = Game.Persistence.playerFile;

        __instance.settingSelectorFullscreen.Populate(Game.Manager.Settings.GetSettingValueNames("fullscreen", false, 0), MathUtils.BoolToInt(playerData.windowMode), false);
        __instance.settingSelectorResolution.Populate(Game.Manager.gameCamera.GetScreenResolutionOptions(), playerData.resolutionIndex, false);

        if (Game.Manager.Ui.currentCanvas.titleCanvas)
        {
            __instance.postGameContainer.alpha = 0f;
            __instance.postGameContainer.blocksRaycasts = false;
            __instance.recordSlotAlphaMode.canvasGroup.alpha = 1f;
            __instance.forfeitCanvasGroup.alpha = 0f;
            __instance.forfeitCanvasGroup.blocksRaycasts = false;
            __instance.forfeitButton.Disable();
            __instance.returnCanvasGroup.alpha = 0f;
            __instance.returnCanvasGroup.blocksRaycasts = false;
            __instance.returnButton.Disable();
            __instance.settingSelectorsContainer.anchoredPosition = __instance.settingSelectorsAltPos;
            return false;
        }

        if (playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_COMPLETE)
        {
            __instance.postGameContainer.alpha = 1f;
            __instance.postGameContainer.blocksRaycasts = true;
            __instance.recordSlotAlphaMode.canvasGroup.alpha = playerFile.GetFlagValue(Flags.ALPHA_MODE) <= 0 ? 1f : 0.5f;
            __instance.recordSlotAlphaMode.Populate(playerFile.alphaDateCount);
            __instance.recordSlotNonStop.Populate(playerFile.nonstopDateCount);
        }
        else
        {
            __instance.postGameContainer.alpha = 0f;
            __instance.postGameContainer.blocksRaycasts = false;
            __instance.recordSlotAlphaMode.canvasGroup.alpha = 1f;
        }

        if (ModInterface.GameState.CurrentState is IPuzzleState puzzleState)
        {
            var puzzle = Game.Session.Puzzle;

            __instance.forfeitCanvasGroup.alpha = 1f;
            __instance.forfeitCanvasGroup.blocksRaycasts = true;
            if (!puzzle.isPuzzleActive
                || !puzzleState.AllowsForfeit
                || puzzle.puzzleStatus.bonusRound
                || puzzle.puzzleGrid.roundOver
                || puzzle.puzzleStatus.gameOver
                || puzzle.dateForfeited)
            {
                __instance.forfeitButton.Disable();
            }
            else
            {
                __instance.forfeitButton.Enable();
            }

            __instance.returnCanvasGroup.alpha = 0f;
            __instance.returnCanvasGroup.blocksRaycasts = false;
            __instance.returnButton.Disable();
        }
        else
        {
            __instance.forfeitCanvasGroup.alpha = 0f;
            __instance.forfeitCanvasGroup.blocksRaycasts = false;
            __instance.forfeitButton.Disable();
            __instance.returnCanvasGroup.alpha = 1f;
            __instance.returnCanvasGroup.blocksRaycasts = true;

            if (ModInterface.GameState.CurrentState.Id == GameStateId.Special)
            {
                __instance.returnButton.Disable();
            }
            else
            {
                __instance.returnButton.Enable();
            }
        }

        __instance.settingSelectorsContainer.anchoredPosition = f_origContainerPos.GetValue<Vector2>(__instance);
        return false;
    }
}