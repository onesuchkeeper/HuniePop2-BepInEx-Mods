using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiWindowActionBubbles))]
internal static class UiWindowActionBubblesPatch
{
    private static FieldInfo _highlightedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_highlightedBubble");
    private static FieldInfo _selectedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_selectedBubble");

    [HarmonyPatch("OnActionBubbleEnter")]
    [HarmonyPrefix]
    private static bool OnActionBubbleEnter(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        //Show stamina hint for talking, since it still is used to limit how much the player can talk at a sim location
        //Hide hint for date, since it doesn't apply to single dates.
        if (State.IsSingleDate
            && (actionBubble.actionBubbleType == ActionBubbleType.DATE || actionBubble.actionBubbleType == ActionBubbleType.TALK))
        {
            _highlightedBubble.SetValue(__instance, actionBubble);

            if (actionBubble.actionBubbleType == ActionBubbleType.TALK)
            {
                Game.Session.Talk.ShowStaminaHint(2, 2);
            }

            Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleOver, null).audioSource.pitch = Random.Range(1f, 2f);

            return false;
        }

        return true;
    }
}
