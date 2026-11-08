using HarmonyLib;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneAppGifts))]
internal static class UiCellphoneAppGiftsStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphoneAppGifts, UiTooltipSimple> Tooltip
        = AccessTools.FieldRefAccess<UiCellphoneAppGifts, UiTooltipSimple>("_tooltip");

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppGifts __instance)
    {
        var app = __instance;
        var state = ModInterface.GameState.CurrentState;
        var status = Game.Session.Puzzle.puzzleStatus;

        Tooltip(app) = Game.Manager.Ui.GetTooltip<UiTooltipSimple>(TooltipType.SIMPLE);

        var pastTutorial = Game.Persistence.playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED;

        // A girl's head dims when gifts can't be given to her. While a puzzle is being played none can, whatever the state allows.
        var dimmed = !state.AllowGifts || (state is IPuzzleState && Game.Session.Puzzle.isPuzzleActive);

        if (state.HasGirlsPresent && pastTutorial && status.girlStatusLeft != null && !status.girlStatusLeft.girlDefinition.specialCharacter)
        {
            app.girlHeadLeftIcon.sprite = status.girlStatusLeft.girlDefinition.GetHead(false);

            if (dimmed)
            {
                app.canvasGroupLeft.alpha = 0.5f;
                app.canvasGroupLeft.blocksRaycasts = false;
            }
        }
        else
        {
            app.canvasGroupLeft.alpha = 0f;
            app.canvasGroupLeft.blocksRaycasts = false;
            app.girlHeadLeft.Disable();
        }

        if (state.HasGirlsPresent && pastTutorial && status.girlStatusRight != null && !status.girlStatusRight.girlDefinition.specialCharacter)
        {
            app.girlHeadRightIcon.sprite = status.girlStatusRight.girlDefinition.GetHead(false);

            if (dimmed)
            {
                app.canvasGroupRight.alpha = 0.5f;
                app.canvasGroupRight.blocksRaycasts = false;
            }
        }
        else
        {
            app.canvasGroupRight.alpha = 0f;
            app.canvasGroupRight.blocksRaycasts = false;
            app.girlHeadRight.Disable();
        }

        EventUtil.Subscribe(app.girlHeadLeft, "ButtonEnterEvent", app, "OnGirlHeadEnter");
        EventUtil.Subscribe(app.girlHeadLeft, "ButtonExitEvent", app, "OnGirlHeadExit");
        EventUtil.Subscribe(app.girlHeadRight, "ButtonEnterEvent", app, "OnGirlHeadEnter");
        EventUtil.Subscribe(app.girlHeadRight, "ButtonExitEvent", app, "OnGirlHeadExit");
        EventUtil.Subscribe(Game.Manager.DragDrop, "DragStartedEvent", app, "OnDragEvent");
        EventUtil.Subscribe(Game.Manager.DragDrop, "DragReleasedEvent", app, "OnDragEvent");
        EventUtil.Subscribe(Game.Manager.DragDrop, "DragEndedEvent", app, "OnDragEvent");

        return false;
    }
}