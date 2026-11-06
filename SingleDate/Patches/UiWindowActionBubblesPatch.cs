using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

/// <summary>Changes the hover behavior of the action bubbles (talk/date) for single dates.</summary>
[HarmonyPatch(typeof(UiWindowActionBubbles))]
internal static class UiWindowActionBubblesPatch
{
    // Private fields of UiWindowActionBubbles: the bubble currently hovered and the tooltip shown with it.
    private static readonly FieldInfo f_highlightedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_highlightedBubble");
    private static readonly FieldInfo f_tooltip = AccessTools.Field(typeof(UiWindowActionBubbles), "_tooltip");

    /// <summary>
    /// Replaces hovering the talk and date bubbles on single dates. Talk shows the stamina hint (stamina is still what
    /// limits how much the player can talk at a sim location). Date doesn't show the usual hint, since it doesn't apply
    /// to single dates, but shows "No Stamina!" if the girl can't go on a date. The base game's version is skipped.
    /// </summary>
    /// <returns>False to skip the original for talk and date bubbles on single dates, true otherwise.</returns>
    [HarmonyPatch("OnActionBubbleEnter")]
    [HarmonyPrefix]
    private static bool OnActionBubbleEnter(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        //Show stamina hint for talking, since it still is used to limit how much the player can talk at a sim location
        //Hide hint for date, since it doesn't apply to single dates.
        if (State.IsSingleDate
            && (actionBubble.actionBubbleType == ActionBubbleType.DATE || actionBubble.actionBubbleType == ActionBubbleType.TALK))
        {
            f_highlightedBubble.SetValue(__instance, actionBubble);

            if (actionBubble.actionBubbleType == ActionBubbleType.TALK)
            {
                Game.Session.Talk.ShowStaminaHint(2, 2);
            }
            else if (actionBubble.actionBubbleType == ActionBubbleType.DATE)
            {
                var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(true);
                if (statusGirl.stamina < 1)
                {
                    var tooltip = f_tooltip.GetValue<UiTooltipSimple>(__instance);
                    tooltip.Populate("No Stamina!", 1, 1f, 1920f);
                    tooltip.Show(Vector2.up * 50f);
                }
            }

            // Hover sound at a random pitch.
            Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleOver, null).audioSource.pitch = Random.Range(1f, 2f);

            return false;
        }

        return true;
    }

    /// <summary>Hides the "No Stamina!" tooltip when the date bubble is left.</summary>
    [HarmonyPatch("OnActionBubbleExit")]
    [HarmonyPostfix]
    private static void OnActionBubbleExit(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        if (actionBubble.actionBubbleType == ActionBubbleType.DATE)
        {
            f_tooltip.GetValue<UiTooltipSimple>(__instance).Hide();
        }
    }
}
