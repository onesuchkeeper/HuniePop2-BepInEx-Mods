using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneAppStatus))]
internal static class UiCellphoneAppStatusStatePatch
{
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, PuzzleStatus> StatusOf = AccessTools.FieldRefAccess<UiCellphoneAppStatus, PuzzleStatus>("_puzzleStatus");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> RelationshipChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_relationshipChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> BaggageChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_baggageChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> ResourceChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_resourceChanged");
    private static readonly AccessTools.FieldRef<UiCellphoneAppStatus, bool> FocusChanged = AccessTools.FieldRefAccess<UiCellphoneAppStatus, bool>("_focusChanged");

    private static readonly MethodInfo m_rollRelationshipSlot = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollRelationshipSlot");
    private static readonly MethodInfo m_rollBaggageSlots = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollBaggageSlots");
    private static readonly MethodInfo m_rollRecoverLines = AccessTools.Method(typeof(UiCellphoneAppStatus), "RollRecoverLines");
    private static readonly MethodInfo m_focusSides = AccessTools.Method(typeof(UiCellphoneAppStatus), "FocusSides");

    /// <summary>The status app has two layouts: the one for a puzzle being played, and the one with the pair and baggage slots.</summary>
    private static bool IsPuzzleLayout => ModInterface.GameState.CurrentState is IPuzzleState;

    [HarmonyPatch(nameof(UiCellphoneAppStatus.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiCellphoneAppStatus __instance)
    {
        var app = __instance;
        var status = StatusOf(app);

        RelationshipChanged(app) = false;
        BaggageChanged(app) = false;
        ResourceChanged(app) = false;
        FocusChanged(app) = false;

        if (!IsPuzzleLayout)
        {
            app.canvasGroupDate.alpha = 1f;
            app.canvasGroupDate.blocksRaycasts = true;

            for (var i = 0; i < app.simCanvasGroups.Length; i++)
            {
                app.simCanvasGroups[i].alpha = 1f;
                app.simCanvasGroups[i].blocksRaycasts = true;
            }

            app.affectionCanvasGroup.alpha = 0f;
            app.affectionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            if (!status.bonusRound)
            {
                app.canvasGroupDate.alpha = 1f;
                app.canvasGroupDate.blocksRaycasts = true;
            }
            else
            {
                app.canvasGroupDate.alpha = 0.5f;
                app.canvasGroupDate.blocksRaycasts = false;
            }

            for (var j = 0; j < app.simCanvasGroups.Length; j++)
            {
                app.simCanvasGroups[j].alpha = 0f;
                app.simCanvasGroups[j].blocksRaycasts = false;
            }

            app.affectionCanvasGroup.alpha = 1f;
            app.affectionCanvasGroup.blocksRaycasts = true;
        }

        app.statusPortraitLeft.Populate(status.girlStatusLeft.girlDefinition, !status.bonusRound);
        app.statusPortraitRight.Populate(status.girlStatusRight.girlDefinition, !status.bonusRound);
        app.affectionMeter.Reset(status.affection, status.affectionGoal);
        app.staminaMeterLeft.Reset(status.girlStatusLeft.stamina, 6);
        app.staminaMeterRight.Reset(status.girlStatusRight.stamina, 6);

        if (!status.bonusRound)
        {
            app.movesRoller.Reset(status.movesRemaining, status.maxMovesRemaining);
            app.passionRollerLeft.Reset(status.girlStatusLeft.passion, 100);
            app.passionRollerRight.Reset(status.girlStatusRight.passion, 100);
            app.sentimentRollerLeft.Reset(status.girlStatusLeft.sentiment, 40);
            app.sentimentRollerRight.Reset(status.girlStatusRight.sentiment, 40);
        }
        else
        {
            app.movesRoller.Lock("");
            app.passionRollerLeft.Lock("");
            app.passionRollerRight.Lock("");
            app.sentimentRollerLeft.Lock("");
            app.sentimentRollerRight.Lock("");
        }

        m_rollRelationshipSlot.Invoke(app, null);
        m_rollBaggageSlots.Invoke(app, null);
        m_rollRecoverLines.Invoke(app, new object[] { true });
        m_focusSides.Invoke(app, new object[] { 0f });

        return false;
    }

    [HarmonyPatch("RollRelationshipSlot")]
    [HarmonyPrefix]
    private static bool RollRelationshipSlot(UiCellphoneAppStatus __instance)
    {
        if (IsPuzzleLayout) return false;

        var pair = Game.Session.Location.currentGirlPair;
        __instance.relationshipSlot.Populate(pair, 0, null, false);
        return false;
    }

    [HarmonyPatch("RollBaggageSlots")]
    [HarmonyPrefix]
    private static bool RollBaggageSlots(UiCellphoneAppStatus __instance)
    {
        if (IsPuzzleLayout) return false;

        var status = StatusOf(__instance);

        for (var i = 0; i < __instance.baggageSlotsLeft.Length; i++)
        {
            __instance.baggageSlotsLeft[i].Populate(status.girlStatusLeft.playerFileGirl);
        }

        for (var j = 0; j < __instance.baggageSlotsRight.Length; j++)
        {
            __instance.baggageSlotsRight[j].Populate(status.girlStatusRight.playerFileGirl);
        }

        return false;
    }
}