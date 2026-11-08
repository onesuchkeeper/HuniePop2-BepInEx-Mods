using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneAppPair))]
internal static class UiCellphoneAppPairStatePatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(UiCellphoneAppPair __instance)
    {
        var app = __instance;
        var state = ModInterface.GameState.CurrentState;

        if (state.AllowsPairApp && Game.Persistence.playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED)
        {
            if (state.HasGirlsPresent)
            {
                var pair = Game.Session.Location.currentGirlPair;
                var status = Game.Session.Puzzle.puzzleStatus;

                app.relationshipSlot.Populate(pair != null && !pair.specialPair ? pair : null, 0, null, false);

                app.pairSideLeft.Populate(status.girlStatusLeft != null && !status.girlStatusLeft.girlDefinition.specialCharacter
                    ? status.girlStatusLeft.playerFileGirl
                    : null);

                app.pairSideRight.Populate(status.girlStatusRight != null && !status.girlStatusRight.girlDefinition.specialCharacter
                    ? status.girlStatusRight.playerFileGirl
                    : null);
            }
            else
            {
                app.sectionTop.alpha = 0f;
                app.sectionTop.blocksRaycasts = false;
            }

            app.levelsTitleBar.Populate(null, app.levelsTitleBarText);

            for (var i = 0; i < app.levelMeters.Length; i++)
            {
                app.levelMeters[i].Populate();
            }
        }
        else
        {
            app.activeCanvasGroup.alpha = 0f;
            app.activeCanvasGroup.blocksRaycasts = false;
            app.inactiveCanvasGroup.alpha = 1f;
            app.inactiveCanvasGroup.blocksRaycasts = true;
            app.ErrorMessage(0);
        }

        EventUtil.Subscribe(app.pairSideLeft, "GirlHeadEnterEvent", app, "OnGirlHeadEnter");
        EventUtil.Subscribe(app.pairSideLeft, "GirlHeadExitEvent", app, "OnGirlHeadExit");
        EventUtil.Subscribe(app.pairSideRight, "GirlHeadEnterEvent", app, "OnGirlHeadEnter");
        EventUtil.Subscribe(app.pairSideRight, "GirlHeadExitEvent", app, "OnGirlHeadExit");

        return false;
    }
}