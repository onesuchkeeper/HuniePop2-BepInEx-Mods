using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiWindowActionBubbles))]
public static class UiWindowActionBubblesPatch
{
    private static FieldInfo _highlightedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_highlightedBubble");

    [HarmonyPatch("OnActionBubbleEnter")]
    [HarmonyPrefix]
    private static bool OnActionBubbleEnter(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        if (State.IsLocationPairSingle()
            && (actionBubble.actionBubbleType == ActionBubbleType.DATE || actionBubble.actionBubbleType == ActionBubbleType.TALK))
        {
            _highlightedBubble.SetValue(__instance, actionBubble);
            //Game.Session.Talk.ShowStaminaHint(-2, 2);
            Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleOver, null).audioSource.pitch = Random.Range(1f, 2f);

            // if (__instance.ActionBubbleEnterEvent != null)
            // {
            //     __instance.ActionBubbleEnterEvent();
            // }
            return false;
        }

        return true;
    }

    // private void OnActionBubbleEnter(UiActionBubble actionBubble)
    // {
    //     this._highlightedBubble = actionBubble;
    //     if (actionBubble.actionBubbleType == ActionBubbleType.TALK)
    //     {
    //         Game.Session.Talk.ShowStaminaHint(2, actionBubble.actionBubbleValue);
    //         PuzzleStatusGirl statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(actionBubble.actionBubbleValue > 0);
    //         if (statusGirl.stamina < 2)
    //         {
    //             this._tooltip.Populate(this.noStaminaTooltipText.Replace("(GIRL)", statusGirl.girlDefinition.girlName), 1, 1f, 1920f);
    //             this._tooltip.Show(Vector2.up * 50f, false);
    //         }
    //     }
    //     else if (actionBubble.actionBubbleType == ActionBubbleType.DATE)
    //     {
    //         Game.Session.Talk.ShowStaminaHint(-2, -1);
    //     }
    //     Game.Manager.Audio.Play(AudioCategory.SOUND, this.sfxBubbleOver, null).audioSource.pitch = Random.Range(1f, 2f);
    //     if (this.ActionBubbleEnterEvent != null)
    //     {
    //         this.ActionBubbleEnterEvent();
    //     }
    // }
}